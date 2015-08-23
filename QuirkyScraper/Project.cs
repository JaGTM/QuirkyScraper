using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class Project : IProject
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public long UnitsSold { get; set; }
        public long InfluencersCount { get; set; }
        public string DevelopmentTime { get; set; }
        public string SubmittedDate { get; set; }
        public string SelectedDate { get; set; }
        public string LaunchedDate { get; set; }
        public IAmazonDetail AmazonDetail { get; set; }
        public IPeople Inventor { get; set; }
        public IEnumerable<ICategory> Categories { get; private set; }

        public Project()
        {
            Categories = new List<ICategory>();
        }

        [JsonConstructor]
        /// <summary>
        /// For usage by Json.net
        /// </summary>
        /// <param name="amazonDetail"></param>
        /// <param name="inventor"></param>
        /// <param name="categories"></param>
        public Project(AmazonDetail amazonDetail, People inventor, IEnumerable<Category> categories)
        {
            AmazonDetail = amazonDetail;
            Inventor = inventor;
            Categories = categories;
        }

        public void AddCategory(ICategory category)
        {
            (Categories as List<ICategory>).Add(category);
        }

        /// <summary>
        /// Checks if another item is the same as the current item
        /// </summary>
        /// <param name="item2"></param>
        /// <returns></returns>
        public bool Equals(IItem item2)
        {
            if (item2 is IProject)
            {
                var project2 = item2 as IProject;
                return string.Equals(this.Name, project2.Name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(this.URL, project2.URL, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
