using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class ContributorsScraper : IScraper
    {
        private string categoryUrl;
        public ContributorsScraper(string categoryUrl)
        {
            this.categoryUrl = categoryUrl;
        }

        public IEnumerable<object> Scrape()
        {
            var driver = Helper.GetWebdriver();
            driver.Navigate().GoToUrl(this.categoryUrl);

            (new WebDriverWait(driver, new TimeSpan(0, 0, 20)))
            .Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.app-views.loadable")));

            try
            {
                var allBut = driver.FindElementByCssSelector("a.nav-inline-item[title='All']");
                allBut.Click();
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine("No all button. Exception: {0}", e);
            }

            (new WebDriverWait(driver, new TimeSpan(0, 0, 10)))
                .Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("div.row.contribution-grid")));

            while (true)
            {
                try
                {
                    var button = (new WebDriverWait(driver, new TimeSpan(0, 0, 10)))
                        .Until(ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("a[data-bb='load-more-button']")));
                    button.Click();
                }
                catch
                {
                    break;
                }
            }

            var contributions = new List<IContribution>();
            foreach (var element in driver.FindElementsByCssSelector("[data-bb='contribution-card-view']"))
            {
                var selected = false;
                try
                {
                    element.FindElement(By.ClassName("picked-icon-container"));
                    selected = true;
                }
                catch
                {
                    selected = false;
                }

                var name = element.FindElement(By.CssSelector("span.card-square-label-overflow")).Text;

                IContribution contribution = new Contribution
                {
                    Contributor = name,
                    Selected = selected
                };
                contributions.Add(contribution);
            }

            driver.Quit();
            return contributions;
        }
    }
}
