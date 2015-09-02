using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class People : IPeople
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string Location { get; set; }
        public IEnumerable<string> Projects { get; private set; }
        public IEnumerable<ICategory> Contributions { get; private set; }
        public IEnumerable<IPeople> Followers { get; private set; }
        public IEnumerable<IPeople> Followings { get; private set; }

        public People()
        {
            Followers = new List<IPeople>();
            Followings = new List<IPeople>();
            Projects = new List<string>();
            Contributions = new List<ICategory>();
        }

        [JsonConstructor]
        /// <summary>
        /// For usage by Json.net
        /// </summary>
        /// <param name="followers"></param>
        /// <param name="followees"></param>
        public People(IEnumerable<People> followers, IEnumerable<People> followees, IEnumerable<string> projects, IEnumerable<Category> contributions)
        {
            Followers = new List<IPeople>(followers);
            Followings = new List<IPeople>(followees);
            Projects = new List<string>(projects);
            Contributions = new List<ICategory>(contributions);
        }

        public void AddProject(string projectName)
        {
            (Projects as List<string>).Add(projectName);
        }

        public void AddContribution(ICategory contributionProject)
        {
            var list = (Contributions as List<ICategory>);

            // Don't add duplicates
            if (list.Any(x => x.Name == contributionProject.Name && x.URL == contributionProject.URL)) return;
            list.Add(contributionProject);
        }

        public void AddFollower(IPeople person)
        {
            var list = (Followers as List<IPeople>);

            // Don't add duplicates
            if (list.Any(x => x.Name == person.Name && x.URL == person.URL)) return;
            list.Add(person);
        }

        public void AddFollowing(IPeople person)
        {
            var list = (Followings as List<IPeople>);

            // Don't add duplicates
            if (list.Any(x => x.Name == person.Name && x.URL == person.URL)) return;
            list.Add(person);
        }
    }
}
