using CarlosAg.ExcelXmlWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    public class ProductContributionProcessor : IProcessor
    {
        public const string DEFAULT_SAVE_PATH = @"D:\Users\JaG\Desktop\productcontributor.xls";

        private IEnumerable<ICategory> categories;
        private string mSavePath;

        public ProductContributionProcessor(IEnumerable<ICategory> categories)
        {
            this.categories = categories;
        }

        public string Savepath
        {
            get {
                if (string.IsNullOrEmpty(mSavePath))
                    mSavePath = DEFAULT_SAVE_PATH;

                return mSavePath;            
            }

            set { mSavePath = value; }
        }

        public void Process()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false
            };
            using (XmlWriter writer = XmlWriter.Create(Savepath, settings))
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

                // Creates the worksheet
                writer.WriteStartElement("Worksheet");
                writer.WriteAttributeString("ss", "Name", null, "Product Network");

                // Creates the table
                writer.WriteStartElement("Table");

                GenerateProductNetwork(writer);

                writer.WriteEndElement();   // Closes table
                writer.WriteEndElement();   // closes worksheet
                
                // Creates the worksheet
                writer.WriteStartElement("Worksheet");
                writer.WriteAttributeString("ss", "Name", null, "Contributor Network");

                // Creates the table
                writer.WriteStartElement("Table");

                GenerateContributionNetwork(writer);

                writer.WriteEndElement();   // Closes table
                writer.WriteEndElement();   // closes worksheet

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
            }
            //var book = new Workbook();
            //book.ExcelWorkbook.ActiveSheetIndex = 1;
            //var productSheet = book.Worksheets.Add("Product Network");
            //var contributorSheet = book.Worksheets.Add("Contributor Network");

            //GenerateProductNetwork(ref productSheet);
            //GenerateContributionNetwork(ref contributorSheet);

            //book.Save(Savepath);
            MessageBox.Show("Excel sheet has been created at " + Savepath + "!");
        }

        private void GenerateContributionNetwork(XmlWriter writer)
        {
            var items = categories.SelectMany(x =>
                x.Contributions.Select(y => new
                {
                    Name = y.Contributor,
                    Project = x.Project
                })
            ).GroupBy(x => x.Name)
            .Select(x => new
            {
                Name = x.Key,
                Projects = x.Select(y => y.Project).Distinct().ToList()
            }).ToList();

            // Build grid
            var grid = new int[items.Count, items.Count];
            for (var row = 0; row < items.Count; row++)
            {
                for (var col = row + 1; col < items.Count; col++)
                {
                    var intersectCount = items[row].Projects.Intersect(items[col].Projects).Count();
                    grid[row, col] = intersectCount;
                }
            }

            // Setup header
            // Creates a row.
            writer.WriteStartElement("Row");
            writer.WriteStartElement("Cell");
            writer.WriteEndElement();
            foreach(var people in items)
                WriteCell(writer, people.Name, true);
            writer.WriteEndElement();

            // Populate data
            for (var row = 0; row < items.Count; row++)
            {
                writer.WriteStartElement("Row");
                WriteCell(writer, items[row].Name, true);

                for (var col = 0; col < items.Count; col++ )
                {
                    if (col == row)
                        WriteCell(writer, "");
                    else if (col < row)
                        WriteCell(writer, grid[col, row].ToString());
                    else
                        WriteCell(writer, grid[row, col].ToString());
                }
                writer.WriteEndElement();
            }
        }

        private void GenerateProductNetwork(XmlWriter writer)
        {
            var items = categories.GroupBy(x => x.Project)
            .Select(x => new
            {
                Project = x.Key,
                Contributors = x.SelectMany(y => y.Contributions.Select(z => z.Contributor).Distinct()).ToList()
            }).ToList();

            // Build grid
            var grid = new int[items.Count, items.Count];
            for (var row = 0; row < items.Count; row++)
            {
                for (var col = row + 1; col < items.Count; col++)
                {
                    var intersectCount = items[row].Contributors.Intersect(items[col].Contributors).Count();
                    grid[row, col] = intersectCount;
                }
            }

            // Setup header
            writer.WriteStartElement("Row");
            writer.WriteStartElement("Cell");
            writer.WriteEndElement();
            foreach (var item in items)
                WriteCell(writer, item.Project, true);
            writer.WriteEndElement();

            // populate data
            for (var row = 0; row < items.Count; row++)
            {
                writer.WriteStartElement("Row");
                WriteCell(writer, items[row].Project, true);

                for (var col = 0; col < items.Count; col++)
                {
                    if (col == row)
                        WriteCell(writer, "");
                    else if (col < row)
                        WriteCell(writer, grid[col, row].ToString());
                    else
                        WriteCell(writer, grid[row, col].ToString());
                }
                writer.WriteEndElement();
            }
        }

        private void WriteCell(XmlWriter writer, string value, bool bold = false)
        {
            writer.WriteStartElement("Cell");
            if(bold)
                writer.WriteAttributeString("ss", "StyleID", null, "s21");

            writer.WriteStartElement("Data");

            writer.WriteAttributeString("ss", "Type", null, "String");
            writer.WriteString(value);

            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void GenerateContributionNetwork(ref Worksheet sheet)
        {
            var items = categories.SelectMany(x =>
                x.Contributions.Select(y => new {
                    Name = y.Contributor,
                    Project = x.Project
                })
            ).GroupBy(x => x.Name)
            .Select(x => new{
                Name = x.Key,
                Projects = x.Select(y => y.Project).Distinct().ToList()
            }).ToList();

            // Build grid
            var grid = new int[items.Count, items.Count];
            for (var row = 0; row < items.Count; row++)
            {
                for (var col = row + 1; col < items.Count; col++)
                {
                    var intersectCount = items[row].Projects.Intersect(items[col].Projects).Count();
                    grid[row, col] = intersectCount;
                }
            }

            // Setup header
            var wsRow = sheet.Table.Rows.Add();
            wsRow.Cells.Add();
            foreach (var people in items)
                wsRow.Cells.Add(new WorksheetCell(people.Name, "HeaderStyle"));

            // Populate data
            for (var row = 0; row < items.Count; row++)
            {
                wsRow = sheet.Table.Rows.Add();
                wsRow.Cells.Add(new WorksheetCell(items[row].Name, "HeaderStyle"));

                for (var col = 0; col < items.Count; col++)
                {
                    if (col == row)
                        wsRow.Cells.Add();  // No value to be shown for this group
                    else if (col < row)
                    {   // In the bottom triangle. Take from the symetric top
                        wsRow.Cells.Add(grid[col, row].ToString());
                    }
                    else
                    {
                        wsRow.Cells.Add(grid[row, col].ToString());
                    }
                }
            }
        }

        private void GenerateProductNetwork(ref Worksheet sheet)
        {
            var items = categories.GroupBy(x => x.Project)
            .Select(x => new
            {
                Project = x.Key,
                Contributors = x.SelectMany(y => y.Contributions.Select(z => z.Contributor).Distinct()).ToList()
            }).ToList();

            // Build grid
            var grid = new int[items.Count, items.Count];
            for (var row = 0; row < items.Count; row++)
            {
                for (var col = row + 1; col < items.Count; col++)
                {
                    var intersectCount = items[row].Contributors.Intersect(items[col].Contributors).Count();
                    grid[row, col] = intersectCount;
                }
            }

            // Setup header
            var wsRow = sheet.Table.Rows.Add();
            wsRow.Cells.Add();
            foreach (var item in items)
                wsRow.Cells.Add(new WorksheetCell(item.Project, "HeaderStyle"));

            // populate data
            for (var row = 0; row < items.Count; row++)
            {
                wsRow = sheet.Table.Rows.Add();
                wsRow.Cells.Add(new WorksheetCell(items[row].Project, "HeaderStyle"));

                for (var col = 0; col < items.Count; col++)
                {
                    if (col == row)
                        wsRow.Cells.Add();  // No value to be shown for this group
                    else if(col < row)
                    {   // In the bottom triangle. Take from the symetric top
                        wsRow.Cells.Add(grid[col, row].ToString());
                    }
                    else
                    {
                        wsRow.Cells.Add(grid[row, col].ToString());
                    }
                }
            }
        }
    }
}
