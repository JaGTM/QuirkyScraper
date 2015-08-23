using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuirkyScraper;
using System.Collections.Generic;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace QuirkyScraperUnitTests
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void GetJsonObjectFromFile_Normal()
        {
            var file = @"C:\Users\JaG\Desktop\projectResults.txt";

            var json = Helper.GetJsonObjectFromFile<List<Mock<IProject>>>(file);

            Assert.IsNotNull(json);
            Assert.IsTrue(json.Count > 0);

            foreach (var item in json)
            {
                Assert.IsInstanceOfType(item.Object, typeof(IProject));
            }
        }

        [TestMethod]
        public void SaveObjectToJson_Normal()
        {
            var testPath = @"C:\Users\JaG\Desktop\testFile.txt";
            try
            {
                File.Delete(testPath);
            }
            catch { }

            var obj = Enumerable.Range(0, 10).Select(x => new AmazonDetail
            {
                ListPrice = x,
                ReviewStars = (x % 5).ToString(),
                URL = "http://q.com?" + x
            });

            Helper.AppendToFile(obj, testPath);

            Thread.Sleep(500);

            Assert.IsTrue(File.Exists(testPath));
            File.Delete(testPath);
        }
    }
}
