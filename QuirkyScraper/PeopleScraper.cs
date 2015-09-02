using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;

namespace QuirkyScraper
{
    public class PeopleScraper : IScraper
    {
        public const string USER_URL_FORMAT = "https://www.quirky.com/users/{0}";

        private List<ICategory> categories;
        public event Action<int, string> ProgressChanged;

        public PeopleScraper(List<ICategory> categories)
        {
            this.categories = categories;
        }

        public IEnumerable<object> Scrape()
        {
            var contributors = new List<IPeople>();
            var totalCount = categories.Count;
            var progress = 0;
            ReportProgress(progress, "Starting people scraping...");

            this.categories = this.categories.OrderBy(x => x.Project).ToList();

            for (var i = 0; i < this.categories.Count; i++)
            {
                var category = categories[i];
                ReportProgress(progress, totalCount,
                    string.Format("Scraping category: {0} ({1})... Contributions: {2} Progress: {3}/{4}", category.Name, category.Project, category.ContributionNum, i, this.categories.Count));

                if (category.ContributionNum == 0) continue;    // Nothing to do here
                var addCategory = new Category
                {
                    Name = category.Name,
                    Project = category.Project,
                    URL = category.URL
                };

                var contributionsString = "https://www.quirky.com/api/v1/inventions/{0}/with_build_interface_objects?with_random_contributions=true";
                var projectId = Regex.Match(category.URL, "(?<=/invent/)[0-9]+(?=/)");

                var json = Helper.GetXHRJson(string.Format(contributionsString, projectId));
                if (json == null) continue;
                
                var jsonObj = JsonConvert.DeserializeObject(json) as JObject;

                var scrapeCount = 0;

                var catDetails = jsonObj["data"]["projects"].FirstOrDefault(x => x.Value<string>("human_name") == category.Name);
                if (catDetails != null)
                {
                    var catId = catDetails.Value<long>("id");
                    var contributionString = "https://www.quirky.com/api/v1/contributions/for_project?parent_id={0}&parent_class=Project&paginated_options%5Bcontributions%5D%5Buse_cursor%5D=true&paginated_options%5Bcontributions%5D%5Bper_page%5D=20&paginated_options%5Bcontributions%5D%5Border_column%5D=created_at&paginated_options%5Bcontributions%5D%5Border%5D=desc";
                    var baseUrl = string.Format(contributionString, catId);

                    json = Helper.GetXHRJson(baseUrl);
                    var additional = "&paginated_options%5Bcontributions%5D%5Bcursor%5D={0}";

                    var hasMore = true;
                    while (hasMore)
                    {
                        jsonObj = JsonConvert.DeserializeObject(json) as JObject;
                        hasMore = jsonObj["paginated_meta"]["contributions"].Value<bool>("has_next_page");
                        var arr = jsonObj["data"].Value<JArray>("contributions");
                        scrapeCount += arr.Count;

                        var cursor = arr.Last().Value<string>("created_at");
                        cursor = Helper.EncodeQuirkyDate(cursor);
                        if (cursor == null) hasMore = false;

                        var url = baseUrl + string.Format(additional, cursor);

                        // Get contributors
                        var users = jsonObj["data"].Value<JArray>("users");
                        foreach (var user in users)
                        {
                            var personName = user.Value<string>("name");
                            var personUrl = string.Format(PeopleScraper.USER_URL_FORMAT, user.Value<string>("id"));

                            var person = contributors.FirstOrDefault(x => x.Name == personName && x.URL == personUrl);
                            if (person == null)
                            {
                                person = new People
                                {
                                    Name = personName,
                                    URL = personUrl
                                };
                                person.AddContribution(addCategory);
                                contributors.Add(person);
                            }
                            else
                                person.AddContribution(addCategory);
                        }

                        ReportProgress(progress, totalCount,
                            string.Format("Scraping category: {0} ({1})... Scraped: {2}/{3} Progress: {4}/{5}", category.Name, category.Project, scrapeCount, category.ContributionNum, i, this.categories.Count));

                        if (hasMore)
                        {
                            json = Helper.GetXHRJson(url);
                        }
                    }
                }

                ReportProgress(++progress, totalCount,
                    string.Format("Completed scraping category: {0} ({1}). Scraped: {2}/{3} Progress: {4}/{5}", category.Name, category.Project, scrapeCount, category.ContributionNum, i, this.categories.Count));
            }

            MessageBox.Show("People scraping completed...");
            return contributors;
        }

        private void ReportProgress(int count, int totalCount, string status = null)
        {
            var progress = count * 100 / totalCount;
            ReportProgress(progress, status);
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}
