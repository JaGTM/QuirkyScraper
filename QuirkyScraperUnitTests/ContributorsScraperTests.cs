using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuirkyScraper;
using System.Collections.Generic;

namespace QuirkyScraperUnitTests
{
    [TestClass]
    public class ContributorsScraperTests
    {
        [TestMethod]
        public void Scrape_Normal()
        {
            var url = "https://www.quirky.com/invent/244110/design";
            var scraper = new ContributorsScraper(url);
            var results = scraper.Scrape();

            Assert.IsNotNull(results);
            Assert.IsInstanceOfType(results, typeof(List<IContribution>));

            var list = results as List<IContribution>;
            Assert.IsTrue(list.Count == 5);
            Assert.AreEqual(list[0].Contributor, "Andrew Erlick");
            Assert.AreEqual(list[1].Contributor, "Andrew Erlick");
            Assert.AreEqual(list[2].Contributor, "Andrew Erlick");
            Assert.AreEqual(list[3].Contributor, "Andrew Erlick");
            Assert.AreEqual(list[4].Contributor, "Andrew Erlick");
            Assert.IsTrue(list[0].Selected);
        }
    }
}
