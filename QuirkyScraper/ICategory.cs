using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public interface ICategory
    {
        string Name { get; set; }
        string URL { get; set; }
        int ContributionNum { get; set; }
        string Project { get; set; }
        IEnumerable<IContribution> Contributions { get; }

        void AddContributions(params IContribution[] categories);
    }
}
