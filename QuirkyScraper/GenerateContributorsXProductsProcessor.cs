using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    public class GenerateContributorsXProductsProcessor:IProcessor
    {
        public const string BASE_FOLDER = @"D:\Users\JaG\Desktop\processedContributionResults\";

        public event Action<int, string> ProgressChanged;
        private List<ICategory> categories;
        private string mSaveFolder;

        public GenerateContributorsXProductsProcessor(List<ICategory> categories)
        {
            this.categories = categories;
        }

        public string SaveFolder
        {
            get
            {
                if (string.IsNullOrEmpty(mSaveFolder))
                    mSaveFolder = BASE_FOLDER;

                return mSaveFolder;
            }

            set { mSaveFolder = value; }
        }

        public void Process()
        {
            if (!Directory.Exists(SaveFolder))
                Directory.CreateDirectory(SaveFolder);

            GenerateGraph();
            MessageBox.Show("Project phases contribution network have been generated in " + SaveFolder);
        }

        private void GenerateGraph()
        {
            var totalCount = 2;
            var count = 0;

            var contributors = categories.SelectMany(x => x.Contributions.Select(y => y.Contributor)).Distinct().ToList();

            totalCount += contributors.Count * 2;
            ReportProgress(++count, totalCount);

            var projects = categories.GroupBy(x => x.Project)
                .Select(x => new Tuple<string, List<string>>(
                    x.Key,
                    x.SelectMany(y => y.Contributions.Select(z => z.Contributor)).Distinct().ToList()
                )).ToList();

            ReportProgress(++count, totalCount);

            
            // Build grid
            var grid = new int[contributors.Count, projects.Count];
            for (var row = 0; row < contributors.Count; row++)
            {
                for (var col = 0; col < projects.Count; col++)
                {
                    var contributed = projects[col].Item2.Any(x => x == contributors[row]);
                    grid[row, col] = contributed ? 1 : 0;
                }
                ReportProgress(++count, totalCount);
            }

            var sizeX = 1000;
            var sizeY = 1000;

            var rowBlocks = (grid.GetLength(0) / sizeY) + 1;
            var colBlocks = (grid.GetLength(1) / sizeX) + 1;
            for (int i = 0; i < rowBlocks; i++)
            {
                for (int j = 0; j < colBlocks; j++)
                {
                    var dimName = i + "_" + j;
                    var filePath = Path.Combine(SaveFolder, ("ContributorXProduct_" + dimName).RemoveInvalidFilePathCharacters() + ".xls");
                    XmlWriter writer = Helper.GenerateXmlWriter(filePath);
                    writer.StartCreateXls()
                        .CreateWorksheet("Product Network " + dimName);

                    var startRowIndex = i * sizeY;
                    var startColIndex = j * sizeX;

                    // Setup header
                    // Creates a row.
                    writer.CreateRow()
                        .WriteCell(string.Empty);
                    for (var index = startColIndex; index < startColIndex + sizeX; index++)
                    {
                        if (index >= projects.Count)
                            break;
                        else
                            writer.WriteCell(projects[index].Item1, true);
                    }
                    writer.CloseRow();

                    // Populate data
                    for (var row = 0; row < sizeY; row++)
                    {
                        var rowIndex = row + startRowIndex;
                        if (rowIndex >= grid.GetLength(0)) break;

                        writer.CreateRow()
                            .WriteCell(contributors[rowIndex], true);

                        for (var col = 0; col < sizeX; col++)
                        {
                            var colIndex = col + startColIndex;
                            if (colIndex >= grid.GetLength(1)) break;

                            writer.WriteCell(grid[rowIndex, colIndex].ToString());
                        }
                        writer.CloseRow();

                        ReportProgress(++count, totalCount);
                    }

                    writer.CloseWorksheet()
                        .CloseXls()
                        .Close();
                }
            }
        }

        private void ReportProgress(int count, int totalCount, string status = null)
        {
            var progress = count * 100 / totalCount;
            ReportProgress(progress, status);
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}
