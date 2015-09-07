using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    public class ProductXContributorsProcessor : IProcessor
    {
        public const string DEFAULT_SAVE_PATH = @"D:\Users\JaG\Desktop\productcontributor.xls";

        public event Action<int, string> ProgressChanged;
        private List<ICategory> categories;
        private string mSavePath;

        public ProductXContributorsProcessor(List<ICategory> categories)
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
            MessageBox.Show("Excel sheet has been created at " + Savepath + "!");
        }

        private void GenerateProductNetwork(XmlWriter writer)
        {
            var items = categories.GroupBy(x => x.Project)
            .Select(x => new
            {
                Project = x.Key,
                Contributors = x.SelectMany(y => y.Contributions.Select(z => z.Contributor).Distinct()).ToList()
            }).ToList();

            // Setup reporting
            var totalCount = items.Count * items.Count * 3 / 2;
            var count = 0;
            var progress = 0;

            // Build grid
            var grid = new int[items.Count, items.Count];
            for (var row = 0; row < items.Count; row++)
            {
                for (var col = row + 1; col < items.Count; col++)
                {
                    var intersectCount = items[row].Contributors.Intersect(items[col].Contributors).Count();
                    grid[row, col] = intersectCount;

                    // Report
                    count++;
                    progress = count * 100 / totalCount;
                    ReportProgress(progress);
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
                    
                    // Report
                    count++;
                    progress = count * 100 / totalCount;
                    ReportProgress(progress);
                }
                writer.CloseRow();
            }
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}
