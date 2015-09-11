using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace QuirkyScraper
{
    public static class Extensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static object FromJson(this string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static IWebElement FindElementByClassName(this IWebDriver driver, string classname)
        {
            return driver.FindElement(By.ClassName(classname));
        }

        public static IWebElement FindElementByCssSelector(this IWebDriver driver, string cssSelector)
        {
            return driver.FindElement(By.CssSelector(cssSelector));
        }

        public static ReadOnlyCollection<IWebElement> FindElementsByCssSelector(this IWebDriver driver, string cssSelector)
        {
            return driver.FindElements(By.CssSelector(cssSelector));
        }

        public static XmlWriter CustomNodes(this XmlWriter writer, Action<XmlWriter> action)
        {
            action(writer);
            return writer;
        }

        public static XmlWriter WriteCell(this XmlWriter writer, string value, bool bold = false)
        {
            writer.WriteStartElement("Cell");
            if (bold)
                writer.WriteAttributeString("ss", "StyleID", null, "s21");

            writer.WriteStartElement("Data");

            writer.WriteAttributeString("ss", "Type", null, "String");
            writer.WriteString(value);

            writer.WriteEndElement();

            writer.WriteEndElement();

            return writer;
        }

        public static XmlWriter CreateRow(this XmlWriter writer)
        {
            writer.WriteStartElement("Row");
            return writer;
        }

        public static XmlWriter CloseRow(this XmlWriter writer)
        {
            writer.WriteEndElement();
            return writer;
        }

        public static XmlWriter CloseXls(this XmlWriter writer)
        {
            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            return writer;
        }

        public static XmlWriter CloseWorksheet(this XmlWriter writer)
        {
            writer.WriteEndElement();   // Closes table
            writer.WriteEndElement();   // closes worksheet
            return writer;
        }

        public static XmlWriter CreateWorksheet(this XmlWriter writer, string sheetName)
        {
            // Creates the worksheet
            writer.WriteStartElement("Worksheet");
            writer.WriteAttributeString("ss", "Name", null, sheetName);

            // Creates the table
            writer.WriteStartElement("Table");
            return writer;
        }

        public static XmlWriter StartCreateXls(this XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteProcessingInstruction("mso-application", "progid='Excel.Sheet'");

            writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
            writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
            writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
            writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
            writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");

            writer.WriteStartElement("DocumentProperties", "urn:schemas-microsoft-com:office:office");
            writer.WriteEndElement();

            // Creates the workbook
            writer.WriteStartElement("ExcelWorkbook", "urn:schemas-microsoft-com:office:excel");
            writer.WriteEndElement();

            // Create header style
            writer.WriteStartElement("Styles");
            writer.WriteStartElement("Style");
            writer.WriteAttributeString("ss", "ID", null, "s21");
            writer.WriteStartElement("Font");
            writer.WriteAttributeString("ss", "Bold", null, "1");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            return writer;
        }

        /// <summary>
        /// Credits to Gary Kindel
        /// http://stackoverflow.com/a/8626562
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="replaceChar"></param>
        /// <returns></returns>
        public static string RemoveInvalidFilePathCharacters(this string filename)
        {
            string replaceChar = "_";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, replaceChar);
        }

        /// <summary>
        /// Attempts to get a date from a string based on the format provided
        /// </summary>
        /// <param name="dateString">String to parse as date</param>
        /// <param name="format">Standard datetime format to parse dateString</param>
        /// <returns></returns>
        public static DateTime? TryGetDate(this string dateString, string format)
        {
            DateTime date;
            if (!DateTime.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out date))
                return null;

            return date;
        }

        /// <summary>
        /// Splits a string into a = string[0:len(string)-frontEnd] and b = string[len(string)-backEnd:end] and return a + b
        /// E.g. string = "Mon 12th Sept", frontEnd = 7, backEnd = 5 => a = "Mon 12", b = " Sept" and returns "Mon 12 Sept"
        /// </summary>
        /// <param name="dateString">string to split</param>
        /// <param name="frontEnd">index, from the back, where the ordinal starts</param>
        /// <param name="backEnd">index, from the back, where the ordinal ends + 1</param>
        /// <returns>Front of ordinal + end of ordinal</returns>
        public static string RemoveOrdinal(this string dateString, int frontEnd, int backEnd)
        {
            var startSplit = dateString.Length - frontEnd;
            var endSplit = dateString.Length - backEnd;
            return dateString.Substring(0, startSplit) + dateString.Substring(endSplit);
        }
    }
}
