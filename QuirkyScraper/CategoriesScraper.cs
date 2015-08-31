using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class CategoriesScraper : IScraper
    {
        private string projectUrl;
        public event Action<int, string> ProgressChanged;
        public CategoriesScraper(string projectUrl)
        {
            this.projectUrl = projectUrl;
        }

        public IEnumerable<object> Scrape()
        {
            var driver = Helper.GetWebdriver();
            driver.Navigate().GoToUrl(this.projectUrl);

            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("product-show-details")));

            var itemDetails = driver.FindElementByClassName("product-show-details");
            var projectName = itemDetails.FindElement(By.CssSelector("h1.product-show-heading")).Text;

            var submissionGridWait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            var submissionGrid = submissionGridWait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("div.row.project-card-grid")));

            var projectCategories = new List<ICategory>();
            foreach (var card in submissionGrid.FindElements(By.CssSelector("a.project-card")))
            {
                var cardUrl = card.GetAttribute("href");
                var cardTitle = card.GetAttribute("title");
                var contributionNumString = card.FindElement(By.CssSelector("div.project-card-icon")).Text;

                int contributorNum;
                if (!int.TryParse(contributionNumString, out contributorNum))
                    contributorNum = -1;

                ICategory category = new Category
                {
                    Name = cardTitle,
                    URL = cardUrl,
                    ContributionNum = contributorNum,
                    Project = projectName
                };
                projectCategories.Add(category);
            }
            driver.Quit();

            return projectCategories;
        }
    }
}
