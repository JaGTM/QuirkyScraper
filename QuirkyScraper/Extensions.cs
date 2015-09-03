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
    }
}
