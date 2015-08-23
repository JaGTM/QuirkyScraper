using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuirkyScraper
{
    public static class Helper
    {
        public static T GetJsonObjectFromFile<T>(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static IWebDriver GetWebdriver()
        {
            var driver = new PhantomJSDriver(@"C:\phantomjs\bin", new PhantomJSOptions() , new TimeSpan(0,2,0));
            return driver;
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private const string DEFAULT_FILEPATH = @"C:\Users\JaG\Desktop\partialResults.txt";

        public static async void AppendToFile(object obj, string filePath = DEFAULT_FILEPATH)
        {
            await Task.Delay(100);
            semaphore.Wait();
            try
            {
                var json = obj.ToJson() + ",";
                File.AppendAllText(filePath, json);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
