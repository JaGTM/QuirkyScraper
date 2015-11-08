using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace QuirkyScraper
{
    public static class Helper
    {
        public enum WebDriverType { FireFox, PhantomJS }

        public static T GetJsonObjectFromFile<T>(string filePath)
        {
            using (StreamReader streamReader = new StreamReader(filePath))
            using(JsonTextReader reader = new JsonTextReader(streamReader))
            {                
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(reader);
            }
        }

        public static IWebDriver GetWebdriver(WebDriverType driverType = WebDriverType.PhantomJS)
        {
            IWebDriver driver = null;
            switch (driverType)
            {
                case WebDriverType.FireFox:
                    driver = new FirefoxDriver();
                    break;

                case WebDriverType.PhantomJS:
                default:
                    driver = new PhantomJSDriver(@"C:\phantomjs\bin", new PhantomJSOptions(), new TimeSpan(0, 2, 0));
                    break;
            }

            return driver;
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private const string DEFAULT_FILEPATH = @"C:\Users\JaG\Desktop\partialResults.txt";

        public static void AppendToFile(object obj, string filePath = DEFAULT_FILEPATH)
        {
            //await Task.Delay(100);
            //semaphore.Wait();
            try
            {
                using (StreamWriter stream = new StreamWriter(filePath, true))
                using (JsonTextWriter writer = new JsonTextWriter(stream))
                {
                    JsonSerializer ser = JsonSerializer.Create(new JsonSerializerSettings{ Formatting = Newtonsoft.Json.Formatting.Indented});
                    ser.Serialize(writer, obj);
                    stream.Write(",");
                }
                //var json = obj.ToJson() + ",";
                //File.AppendAllText(filePath, json);
            }
            finally
            {
                //semaphore.Release();
            }
        }

        internal static T GetJsonArrayFromTemp<T>(string filePath)
        {
            var tempFilePath = Helper.CloneTextFileAndAddArray(filePath);
            T obj = default(T);

            using (StreamReader streamReader = new StreamReader(tempFilePath))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                JsonSerializer ser = new JsonSerializer();
                obj = ser.Deserialize<T>(reader);
            }

            File.Delete(tempFilePath);
            return obj;
        }

        public static IEnumerable<T> GetJsonIEnumerableFromTemp<T>(string filePath)
        {
            var tempFilePath = Helper.CloneTextFileAndAddArray(filePath);
            return Helper.DeserializeIEnumerableOf<T>(tempFilePath);
        }

        public static IEnumerable<T> DeserializeIEnumerableOf<T>(string filePath)
        {
            using (TextReader textReader = new StreamReader(filePath))
            {
                using (var reader = new JsonTextReader(textReader))
                {
                    var cachedDeserializer = JsonSerializer.Create();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            var deserializedItem = cachedDeserializer.Deserialize<T>(reader);
                            yield return deserializedItem;
                        }
                    }
                }
            }
        }

        public static string CloneTextFileAndAddArray(string filePath)
        {
            var tempFile = filePath + "_clone";
            using (var readStream = File.OpenRead(filePath))
            using (var reader = new StreamReader(readStream))
            using (var writeStream = File.OpenWrite(tempFile))
            using (var writer = new StreamWriter(writeStream))
            {
                writer.Write("[");
                while (reader.Peek() >= 0)
                {
                    var buffer = new char[4096];
                    reader.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer);
                }
                writer.Write("]");
            }

            return tempFile;
        }

        public static XmlWriter GenerateXmlWriter(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false
            };
            return XmlWriter.Create(path, settings);
        }

        public static string GetXHRJson(string url)
        {
            string json = null;
            int count = 0;
            while (json == null)
            {
                try
                {
                    json = Helper.GetResponseString(url);
                    if (json != null || count++ > 1)
                        return json;
                }
                catch (WebException e)
                {
                    if (e.Message == "The remote server returned an error: (401) Unauthorized.")    // User no longer exits
                        throw new ArgumentOutOfRangeException("Invalid user");

                    // Failed once. Try again
                    Console.WriteLine("Failed. Exception: {0}", e);
                    if (count++ > 2) return null; // If failed too many times, just return null
                }
                catch (Exception e)
                {   // Failed once. Try again
                    Console.WriteLine("Failed. Exception: {0}", e);
                    if (count++ > 2) return null; // If failed too many times, just return null
                }
            }

            return null;    // Bo bian
        }

        public static string GetResponseString(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.KeepAlive = false;
            req.ProtocolVersion = HttpVersion.Version10;

            HttpWebResponse resp = null;
            StreamReader reader = null;
            using (resp = (HttpWebResponse)req.GetResponse())
            using (reader = new StreamReader(resp.GetResponseStream(),
                     Encoding.ASCII))
            {
                return reader.ReadToEnd();
            }
        }

        public static string EncodeQuirkyDate(string responseDate)
        {
            DateTime date;
            if (!DateTime.TryParseExact(responseDate, "MM/dd/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal, out date))
                return null;
            
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var convertedTime = TimeZoneInfo.ConvertTime(date, easternZone);
            var tzOffset = easternZone.GetUtcOffset(convertedTime);
            var parsedDateTimeZone = new DateTimeOffset(convertedTime, tzOffset);
            return HttpUtility.UrlEncode(parsedDateTimeZone.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz"));
        }
    }
}
