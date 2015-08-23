using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public interface IPeople
    {
        string Name { get; set; }
        string URL { get; set; }
        string Location { get; set; }
        IEnumerable<string> Projects { get; }
        IEnumerable<string> Contributions { get; }
        void AddProject(string projectName);
        void AddContribution(string contributionProjectName);
        IEnumerable<IPeople> Followers { get; }
        IEnumerable<IPeople> Followees { get; }
    }
}
