using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace QuirkyScraper
{
    internal class SpecialistsScraper : IScraper
    {
        public event Action<int, string> ProgressChanged;

        public IEnumerable<object> Scrape()
        {
            // Get skills
            List<string> skills = GetSkills();
            
            List<IPeople> people = new List<IPeople>();
            ReportProgress(0, "Begin retrieving people for each skill...");
            for(var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                // Get people with skill
                ReportProgress(i, skills.Count, string.Format("Retrieving people with {0} skill...", skill));
                GetPeople(skill, i, skills.Count, ref people);
                ReportProgress(i + 1, skills.Count, string.Format("Completed retrieving people with {0} skills.", skill));
            }

            MessageBox.Show("Specialists scraping completed.");
            return people;
        }

        private void GetPeople(string skill, int index, int totalCount, ref List<IPeople> people)
        {
            var baseUrl = "https://www.quirky.com/api/v1/users/by_skill?skill={0}&paginated_options%5Busers%5D%5Bpage%5D={1}&paginated_options%5Busers%5D%5Bper_page%5D=20&paginated_options%5Busers%5D%5Border_column%5D=id&paginated_options%5Busers%5D%5Border%5D=asc";
            var hasMore = true;
            var page = 1;
            var scrapedCount = 0;

            while (hasMore)
            {
                var url = string.Format(baseUrl, skill, page++);
                var json = Helper.GetXHRJson(url);
                var jsonObj = json.FromJson<JObject>();

                var stats = jsonObj["paginated_meta"]["users"];
                hasMore = stats.Value<bool>("has_next_page");
                var total = stats.Value<int>("total");  // The number of people with this skill
                
                // Get followers
                var users = jsonObj["data"].Value<JArray>("users");
                scrapedCount += users.Count;
                foreach (var user in users)
                {
                    var personName = user.Value<string>("name");
                    var personUrl = string.Format(PeopleScraper.USER_URL_FORMAT, user.Value<string>("id"));

                    var person = people.FirstOrDefault(x => x.Name == personName && x.URL == personUrl);
                    if (person == null)
                    {
                        person = new People
                        {
                            Name = personName,
                            URL = personUrl
                        };
                        people.Add(person);
                    }
                    person.AddSkill(skill);
                }

                ReportProgress(index, totalCount,
                    string.Format("Scraping people with {0} skill... Scraped: {1}/{2}. Progress: {3}/{4}", skill, scrapedCount, total, index, totalCount));
            }

            ReportProgress(index + 1, totalCount,
                string.Format("Completed scraping people with {0} skill. Scraped: {1}. Progress: {2}/{3}", skill, scrapedCount, index + 1, totalCount));
        }

        private List<string> GetSkills()
        {
            ReportProgress(0, "Retrieving available skills...");
            var html = Helper.GetResponseString("https://www.quirky.com/community");
            var json = Regex.Match(html, "(?<=\"SKILLS\":)\\[[^\\]]+\\]").ToString();

            var skills = json.FromJson<List<string>>();
            ReportProgress(0, string.Format("Completed retrieving skills. Skills Count: {0}", skills.Count));

            return skills;
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