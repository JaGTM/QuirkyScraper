using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class ParticipantScraper : IScraper
    {
        private const string FILE_PATH = @"C:\Users\JaG\Desktop\pcats.txt";
        private IEnumerable<string> projectUrls;
        public event Action<int, string> ProgressChanged;

        public ParticipantScraper(IEnumerable<string> projectUrls)
        {
            this.projectUrls = projectUrls;
        }

        public IEnumerable<object> Scrape()
        {
            // Scrape categories
            List<ICategory> projectCategories = null;
            var catFilePath = @"C:\Users\JaG\Desktop\cats.txt";
            try
            {
                projectCategories = Helper.GetJsonObjectFromFile<List<Category>>(catFilePath).Cast<ICategory>().ToList();
            }
            catch { }

            if (projectCategories == null)
            {
                projectCategories = GetAllProjectsCategories();
                Helper.AppendToFile(projectCategories, catFilePath);
            }

            List<ICategory> existingCategories = GetExistingProcessCategories();
            if (existingCategories.Count > 0)
                projectCategories = projectCategories
                    .Where(x => !existingCategories.Any(y => y.Name == x.Name && y.URL == x.URL))
                    .ToList();

            List<ICategory> processedCategories = FillParticipantsInCategories(projectCategories);
            return processedCategories;
        }

        public static List<ICategory> GetNoDuplicateProcessedCategories()
        {
            var cats = GetExistingProcessCategories();
            var results = new List<ICategory>();
            foreach (var cat in cats)
            {
                if (!results.Any(x => x.Name == cat.Name && x.URL == cat.URL))
                    results.Add(cat);
            }

            var notInResults = cats.Except(results).ToList();

            return results;
        }

        public static List<ICategory> GetExistingProcessCategories(string filePath = null)
        {
            if (filePath == null)
                filePath = ParticipantScraper.FILE_PATH;

            string text = null;
            try
            {
                text = File.ReadAllText(filePath);
                var items = JsonConvert.DeserializeObject<List<Category>>("[" + text.Substring(0, text.Length - 1) + "]");
                return items.Cast<ICategory>().ToList();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return new List<ICategory>();
            }
        }

        private List<ICategory> FillParticipantsInCategories(List<ICategory> projectCategories)
        {
            List<ICategory> categories = new List<ICategory>();
            foreach (var category in projectCategories.OrderBy(x => x.ContributionNum))
            {
                if (category.ContributionNum > 0)
                {
                    try
                    {
                        IScraper contScraper = new ContributorsScraper(category.URL);
                        var results = contScraper.Scrape();
                        category.AddContributions(results.Cast<IContribution>().ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed. Exception {0}", e);
                        throw e;
                    }
                }
                Helper.AppendToFile(category, ParticipantScraper.FILE_PATH);
                categories.Add(category);
            }
            return categories;
        }

        private List<ICategory> GetAllProjectsCategories()
        {
            List<ICategory> categories = new List<ICategory>();
            foreach (var projectURL in this.projectUrls)
            {
                IScraper catScraper = new CategoriesScraper(projectURL);
                var results = catScraper.Scrape();
                categories.AddRange(results.Cast<ICategory>());
            }
            return categories;
        }


    }
}
