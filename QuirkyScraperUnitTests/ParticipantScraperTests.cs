using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuirkyScraper;
using System.IO;
using Newtonsoft.Json;

namespace QuirkyScraperUnitTests
{
    [TestClass]
    public class ParticipantScraperTests
    {
        [TestMethod]
        public void GetDistinctProcessed_Normal()
        {
            var cats = ParticipantScraper.GetNoDuplicateProcessedCategories();
        }
    }
}
