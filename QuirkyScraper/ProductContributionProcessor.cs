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
        public const string BASE_FOLDER = @"D:\Users\JaG\Desktop\processedResults\";

        private IEnumerable<ICategory> categories;
        private string mSavePath;

        public ProductContributionProcessor(IEnumerable<ICategory> categories)
        {
            this.categories = categories;
        }

        public string Savepath
        {
            get
            {
                if (string.IsNullOrEmpty(mSavePath))
                    mSavePath = DEFAULT_SAVE_PATH;

                return mSavePath;
            }

            set { mSavePath = value; }
        }

        public void Process()
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()

                .CreateWorksheet("Product Network")
                .CustomNodes(GenerateProductNetwork)
                .CloseWorksheet()
                .CloseXls();
            }

            GenerateContributionNetwork();
            MessageBox.Show("Excel sheet has been created at " + Savepath + "!");
        }

        private void GenerateContributionNetwork()
        {
            var items = categories.SelectMany(x =>
                x.Contributions.Select(y => new
                {
                    Name = y.Contributor,
                    Project = x.Project
                })
            ).GroupBy(x => x.Name)
            .Select(x => new Tuple<string, List<string>>
            (
                x.Key,
                x.Select(y => y.Project).Distinct().ToList()
            )).ToList();

            // Build grid
            var grid = new int[items.Count, items.Count];
            for (var row = 0; row < items.Count; row++)
            {
                for (var col = row + 1; col < items.Count; col++)
                {
                    var intersectCount = items[row].Item2.Intersect(items[col].Item2).Count();
                    grid[row, col] = intersectCount;
                }
            }

            SaveContributionNetwork(grid, items);
        }

        private void SaveContributionNetwork(int[,] grid, List<Tuple<string, List<string>>> items)
        {
            var sizeX = 1000;
            var sizeY = 1000;

            var rowBlocks = (grid.GetLength(0) / sizeY) + 1;
            var colBlocks = (grid.GetLength(1) / sizeX) + 1;
            for(int i = 0; i < rowBlocks; i++)
            {
                for(int j = 0; j < colBlocks; j++)
                {
                    var dimName = i + "_" + j;
                    XmlWriter writer = Helper.GenerateXmlWriter(BASE_FOLDER + "ContributorNetwork_" + dimName + ".xls");
                    writer.StartCreateXls()
                        .CreateWorksheet("Contributor Network " + dimName);

                    var startRowIndex = i * sizeY;
                    var startColIndex = j * sizeX;
                    
                    // Setup header
                    // Creates a row.
                    writer.CreateRow()
                        .WriteCell(string.Empty);
                    for (var peopleIndex = startColIndex; peopleIndex < startColIndex + sizeX; peopleIndex++)
                    {
                        if (peopleIndex >= items.Count)
                            break;
                        else
                            writer.WriteCell(items[peopleIndex].Item1, true);
                    }
                    writer.CloseRow();

                    // Populate data
                    for(var row = 0; row < sizeY; row++){
                        var rowIndex = row + startRowIndex;
                        if (rowIndex >= grid.GetLength(0)) break;

                        writer.CreateRow()
                            .WriteCell(items[rowIndex].Item1, true);

                        for(var col = 0; col < sizeX; col++)
                        {
                            var colIndex = col + startColIndex;
                            if (colIndex >= grid.GetLength(1)) break;
                            
                            if (colIndex == rowIndex)
                                writer.WriteCell("");
                            else if (colIndex < rowIndex)
                                writer.WriteCell(grid[colIndex, rowIndex].ToString());
                            else
                                writer.WriteCell(grid[rowIndex, colIndex].ToString());
                        }
                        writer.CloseRow();
                    }
                                        
                    writer.CloseWorksheet()
                        .CloseXls()
                        .Close();
                }
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
            writer.CreateRow()
                .WriteCell(string.Empty);
            foreach (var item in items)
                writer.WriteCell(item.Project, true);
            writer.CloseRow();

            // populate data
            for (var row = 0; row < items.Count; row++)
            {
                writer.CreateRow()
                    .WriteCell(items[row].Project, true);

                for (var col = 0; col < items.Count; col++)
                {
                    if (col == row)
                        writer.WriteCell(string.Empty);
                    else if (col < row)
                        writer.WriteCell(grid[col, row].ToString());
                    else
                        writer.WriteCell(grid[row, col].ToString());
                }
                writer.CloseRow();
            }
        }

        private void GenerateContributionNetwork(ref Worksheet sheet)
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
    }
}
