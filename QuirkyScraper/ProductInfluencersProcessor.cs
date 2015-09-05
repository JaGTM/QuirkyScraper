using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    public class ProductInfluencersProcessor : IProcessor
    {
        public const string DEFAULT_SAVE_PATH = @"D:\Users\JaG\Desktop\productInfluencers.xls";

        private List<ICategory> categories;
        private string mSavePath;
        public event Action<int, string> ProgressChanged;

        public ProductInfluencersProcessor(List<ICategory> categories)
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
            var totalCount = 1;
            var count = 0;

            var items = categories.GroupBy(x => x.Project)
            .Select(x => new
            {
                Project = x.Key,
                Influencers = x.SelectMany(y => y.Contributions.Select(z => z.Contributor)).Distinct().Count()
            }).ToList();

            ReportProgress(++count, totalCount);
            totalCount += items.Count;

            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                .CreateWorksheet("ProjectOverview")
                .CreateRow()
                .WriteCell("Project Name", true)
                .WriteCell("Total Influencers", true)
                .CloseRow();

                for(int i = 0; i < items.Count; i++)
                {
                    writer.CreateRow()
                        .WriteCell(items[i].Project)
                        .WriteCell(items[i].Influencers.ToString())
                        .CloseRow();

                    ReportProgress(++count, totalCount);
                }

                writer.CloseWorksheet()
                .CloseXls();
            }
            MessageBox.Show("Excel sheet has been created at " + Savepath + "!");
        }

        private void ReportProgress(int count, int totalCount)
        {
            var progress = count * 100 / totalCount;
            ReportProgress(progress);
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}
