using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace QuirkyScraper
{
    internal class FollowerFolloweeScraper : IScraper
    {
        private List<People> people;
        public event Action<int, string> ProgressChanged;

        public FollowerFolloweeScraper(List<People> people)
        {
            this.people = people;
        }

        public IEnumerable<object> Scrape()
        {
            ReportProgress(0, "Starting follower/followee scraping...");
            var results = new List<IPeople>();
            for(int i = 0; i < this.people.Count; i++)
            {
                var person = this.people[i];
                ReportProgress(i, this.people.Count, string.Format("Scraping {0}'s followers...", person.Name));
                PopulateFollowers(i, this.people.Count, ref person);
                ReportProgress(i, this.people.Count, string.Format("Scraping {0}'s followees...", person.Name));
                PopulateFollowees(i, this.people.Count, ref person);
                results.Add(person);
                ReportProgress(i + 1, this.people.Count, string.Format("Completed scraping {0}'s followers and followees...", person.Name));
            }

            MessageBox.Show("Follower and followee scraping completed.");
            return results;
        }

        private void PopulateFollowees(int index, int totalCount, ref People person)
        {
            Populate(index, totalCount, ref person, false);
        }

        private void PopulateFollowers(int index, int totalCount, ref People person)
        {
            Populate(index, totalCount, ref person, true);
        }

        private void Populate(int index, int totalCount, ref People person, bool isFollower)
        {
            var personId = Regex.Match(person.URL, "(?<=users/)[0-9]+");

            var reportText = isFollower ? "followers" : "followees";

            var urlpage = isFollower ? "following" : "followers";
            var urlBase = "https://www.quirky.com/api/v1/user_profile/{0}/{1}?paginated_options%5Bfollows%5D%5Buse_cursor%5D=true&paginated_options%5Bfollows%5D%5Bper_page%5D=20&paginated_options%5Bfollows%5D%5Border_column%5D=created_at&paginated_options%5Bfollows%5D%5Border%5D=desc";
            var baseUrl = string.Format(urlBase, personId, urlpage);
            var urlCursorAddition = "&paginated_options%5Bfollows%5D%5Bcursor%5D={0}";

            var hasMore = true;
            var firstIteration = true;

            string cursor = null;
            var scrapedCount = 0;

            while (hasMore)
            {
                var url = baseUrl;
                if (firstIteration) firstIteration = false; // First iteration has no pagination cursor
                else url += string.Format(urlCursorAddition, cursor);
                
                var json = Helper.GetXHRJson(url);

                var jsonObj = JsonConvert.DeserializeObject(json) as JObject;

                hasMore = jsonObj["paginated_meta"]["follows"].Value<bool>("has_next_page");
                var arr = jsonObj["data"].Value<JArray>("follows");
                scrapedCount += arr.Count;

                if (hasMore)
                {
                    cursor = arr.First.Value<string>("created_at");
                    cursor = Helper.EncodeQuirkyDate(cursor);
                    if (cursor == null) hasMore = false;
                }

                // Get followers
                var users = jsonObj["data"].Value<JArray>("users");
                foreach (var user in users)
                {
                    var personName = user.Value<string>("name");
                    var personUrl = string.Format(PeopleScraper.USER_URL_FORMAT, user.Value<string>("id"));
                    
                    var fellow = new People
                    {
                        Name = personName,
                        URL = personUrl
                    };
                    if (isFollower)
                        person.AddFollower(fellow);
                    else
                        person.AddFollowee(fellow);
                }

                ReportProgress(index, totalCount,
                    string.Format("Scraping {0}'s {1}... Scraped: {2} {1}. Progress: {3}/{4}", person.Name, reportText, scrapedCount, index, totalCount));
            }

            ReportProgress(index, totalCount,
                string.Format("Completed scraping {0}'s {1}. Scraped: {2} {1}. Progress: {3}/{4}", person.Name, reportText, scrapedCount, index, totalCount));
        }

        private void ReportProgress(int count, int totalCount, string status = null)
        {
            ReportProgress(count * 100 / totalCount, status);
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}