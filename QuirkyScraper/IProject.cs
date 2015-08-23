using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public interface IProject: IItem
    {
        string Name { get; set; }
        string URL { get; set; }
        long UnitsSold { get; set; }
        long InfluencersCount { get; set; }
        string DevelopmentTime { get; set; }
        string SubmittedDate { get; set; }
        string SelectedDate { get; set; }
        string LaunchedDate { get; set; }
        IAmazonDetail AmazonDetail { get; set; }
        IPeople Inventor { get; set; }
        IEnumerable<ICategory> Categories { get; }

        void AddCategory(ICategory category);
    }
}
