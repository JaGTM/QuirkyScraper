using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace QuirkyScraper
{
    internal class FollowerFollowingScraper : IScraper
    {
        private const string DEFAULT_TEMP_FILE_PATH = @"tempFollowerFollowing.txt";

        private List<People> people;
        private string mTempFilePath;

        public event Action<int, string> ProgressChanged;

        public FollowerFollowingScraper(List<People> people)
        {
            this.people = people;
        }

        public string TempFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(mTempFilePath))
                    mTempFilePath = DEFAULT_TEMP_FILE_PATH;
                return mTempFilePath;
            }
            set
            {
                mTempFilePath = value;
            }
        }

        public IEnumerable<object> Scrape()
        {
            ReportProgress(0, "Starting follower/following scraping...");
            var tempPeople = GetTempPeople();

            var results = new List<IPeople>();
            //tempPeople.ForEach(x => results.Add(x));    // Add already processed people

            Stack<IPeople> stack = new Stack<IPeople>(this.people);
            this.people = null;
            int totalPeopleToScrap = stack.Count;
            for (int i = 0; i < totalPeopleToScrap; i++)
            {
                IPeople person = stack.Pop();
                if (tempPeople.Any(x => x.Name == person.Name && x.URL == person.URL))
                {
                    Console.WriteLine("This person already scraped: {0}", person.Name);
                    continue;
                }

                var personId = Regex.Match(person.URL, "(?<=users/)[0-9]+").ToString();

                int followers = -1, followings = -1;
                try
                {
                    GetFollowersFolloweesCount(out followers, out followings, personId);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    if (!string.IsNullOrEmpty(e.ParamName) && string.Equals(e.ParamName, "Invalid user", StringComparison.OrdinalIgnoreCase))
                        continue;   // User no longer exist continue
                }

                ReportProgress(i, totalPeopleToScrap, string.Format("Scraping {0}'s followers... {1}/{2} completed.", person.Name, i, totalPeopleToScrap));
                PopulateFollowers(i, totalPeopleToScrap, ref person, personId, followers);
                ReportProgress(i, totalPeopleToScrap, string.Format("Scraping {0}'s followings... {1}/{2} completed.", person.Name, i, totalPeopleToScrap));
                PopulateFollowings(i, totalPeopleToScrap, ref person, personId, followings);
                ReportProgress(i, totalPeopleToScrap, string.Format("Writing {0} to temp file... {1}/{2} scraping completed.", person.Name, i + 1, totalPeopleToScrap));
                WriteToTemp(person);
                ReportProgress(i + 1, totalPeopleToScrap, string.Format("Completed scraping {0}'s followers and followings. {1}/{2} completed.", person.Name, i + 1, totalPeopleToScrap));
            }

            MessageBox.Show("Follower and following scraping completed.");
            return results;
        }

        private void WriteToTemp(IPeople person)
        {
            Helper.AppendToFile(person, TempFilePath);
        }

        private List<IPeople> GetTempPeople()
        {
            if (!File.Exists(TempFilePath)) return new List<IPeople>();

            try
            {
                var people = Helper.GetJsonIEnumerableFromTemp<People>(TempFilePath);
                return people.Select(x => new People { Name = x.Name, URL = x.URL } as IPeople).ToList();
                //return Helper.GetJsonArrayFromTemp<List<People>>(TempFilePath).Cast<IPeople>().ToList();
                //return Helper.GetJsonObjectFromFile<List<People>>(TempFilePath).Cast<IPeople>().ToList();
            }
            catch
            {   // If some how file is corrupted/unreadable get new list
                return new List<IPeople>();
            }
        }

        private void GetFollowersFolloweesCount(out int followersCount, out int followingCount, string personId)
        {
            var baseUserUrl = "https://www.quirky.com/api/v1/user_profile/{0}/submitted_inventions?paginated_options%5Binventions%5D%5Buse_cursor%5D=true&paginated_options%5Binventions%5D%5Bper_page%5D=12&paginated_options%5Binventions%5D%5Border_column%5D=created_at&paginated_options%5Binventions%5D%5Border%5D=desc";
            var json = Helper.GetXHRJson(string.Format(baseUserUrl, personId));
            var jsonObj = JsonConvert.DeserializeObject(json) as JObject;
            var counters = jsonObj["data"]["user"]["counters"];
            followersCount = counters.Value<int>("followers_count");
            followingCount = counters.Value<int>("following_count");
        }

        private void PopulateFollowings(int index, int totalCount, ref IPeople person, string personId, int count)
        {
            Populate(index, totalCount, ref person, false, personId, count);
        }

        private void PopulateFollowers(int index, int totalCount, ref IPeople person, string personId, int count)
        {
            Populate(index, totalCount, ref person, true, personId, count);
        }

        private void Populate(int index, int totalCount, ref IPeople person, bool isFollower, string personId, int count)
        {
            var reportText = isFollower ? "followers" : "followings";

            var urlpage = isFollower ? "followers" : "following";
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
                    cursor = arr.Last.Value<string>("created_at");
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
                        person.AddFollowing(fellow);
                }

                ReportProgress(index, totalCount,
                    string.Format("Scraping {0}'s {1}... Scraped: {2}/{3} {1}. Progress: {4}/{5}", person.Name, reportText, scrapedCount, count, index, totalCount));
            }

            ReportProgress(index + 1, totalCount,
                string.Format("Completed scraping {0}'s {1}. Scraped: {2}/{3} {1}. Progress: {4}/{5}", person.Name, reportText, scrapedCount, count, index + 1, totalCount));
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