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

        [ExpectedException(typeof(ArgumentOutOfRangeException), "Invalid user")]
        [TestMethod]
        public void GetXHRJson_ThrowsCorrectlyWhenUnauthorized()
        {
            var baseUserUrl = "https://www.quirky.com/api/v1/user_profile/{0}/submitted_inventions?paginated_options%5Binventions%5D%5Buse_cursor%5D=true&paginated_options%5Binventions%5D%5Bper_page%5D=12&paginated_options%5Binventions%5D%5Border_column%5D=created_at&paginated_options%5Binventions%5D%5Border%5D=desc";
            var personId = 672272;
            var testURL = string.Format(baseUserUrl, personId);
            Helper.GetXHRJson(testURL);
        }
    }
}
