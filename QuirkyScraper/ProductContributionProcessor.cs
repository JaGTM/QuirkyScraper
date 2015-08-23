using CarlosAg.ExcelXmlWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace QuirkyScraper
{
    public class ProductContributionProcessor : IProcessor
    {
        public const string DEFAULT_SAVE_PATH = @"C:\Users\JaG\Desktop\productcontributor.xls";

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
            var book = new Workbook();
            book.ExcelWorkbook.ActiveSheetIndex = 1;
            var productSheet = book.Worksheets.Add("Product Network");
            var contributorSheet = book.Worksheets.Add("Contributor Network");

            //GenerateProductNetwork(ref productSheet);
            GenerateContributionNetwork(ref contributorSheet);

            book.Save(Savepath);
            MessageBox.Show("Excel sheet has been created at " + Savepath + "!");
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
