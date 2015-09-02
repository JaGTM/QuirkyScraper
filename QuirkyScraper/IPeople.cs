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
        IEnumerable<ICategory> Contributions { get; }
        void AddProject(string projectName);
        void AddContribution(ICategory contributionProjectName);
        IEnumerable<IPeople> Followers { get; }
        IEnumerable<IPeople> Followings { get; }
        void AddFollower(IPeople person);
        void AddFollowing(IPeople person);
    }
}
