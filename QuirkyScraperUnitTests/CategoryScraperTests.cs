using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuirkyScraper;
using System.Collections.Generic;

namespace QuirkyScraperUnitTests
{
    [TestClass]
    public class CategoryScraperTests
    {
        [TestMethod]
        public void Scrape_Normal()
        {
            var url = "https://www.quirky.com/invent/244110";
            var scraper = new CategoriesScraper(url);
            var result = scraper.Scrape();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<ICategory>));

            var list = result as List<ICategory>;
            Assert.IsTrue(list.Count == 5);
            Assert.AreEqual(list[0].Name, "Design");
            Assert.AreEqual(list[1].Name, "Tagline");
            Assert.AreEqual(list[2].Name, "Problem / Solution");
            Assert.AreEqual(list[3].Name, "Naming");
            Assert.AreEqual(list[4].Name, "Images");
        }
    }
}
