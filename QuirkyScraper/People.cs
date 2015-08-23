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
        public IEnumerable<string> Contributions { get; private set; }
        public IEnumerable<IPeople> Followers { get; private set; }
        public IEnumerable<IPeople> Followees { get; private set; }

        public People()
        {
            Followers = new List<IPeople>();
            Followees = new List<IPeople>();
            Projects = new List<string>();
            Contributions = new List<string>();
        }

        [JsonConstructor]
        /// <summary>
        /// For usage by Json.net
        /// </summary>
        /// <param name="followers"></param>
        /// <param name="followees"></param>
        public People(IEnumerable<People> followers, IEnumerable<People> followees)
        {
            Followers = followers;
            Followees = followees;
            Projects = new List<string>();
            Contributions = new List<string>();
        }

        public void AddProject(string projectName)
        {
            (Projects as List<string>).Add(projectName);
        }

        public void AddContribution(string contributionProjectName)
        {
            (Contributions as List<string>).Add(contributionProjectName);
        }
    }
}
