using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class Category : ICategory
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public int ContributionNum { get; set; }
        public string Project { get; set; }
        public IEnumerable<IContribution> Contributions { get; private set; }

        /// <summary>
        /// For usage by Json.net
        /// </summary>
        /// <param name="contributions"></param>
        [JsonConstructor]
        public Category(IEnumerable<Contribution> contributions)
        {
            if(contributions != null)
                Contributions = new List<IContribution>(contributions);
        }

        public Category()
        {
            Contributions = new List<IContribution>();
        }

        public void AddContributions(params IContribution[] contributions)
        {
            (Contributions as List<IContribution>).AddRange(contributions);
        }
    }
}
