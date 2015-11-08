using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace QuirkyScraper.Processors
{
    class PhaseUniqueContributorsProcessor : Processor
    {
        private List<ICategory> categories;

        public PhaseUniqueContributorsProcessor(List<ICategory> categories)
        {
            this.categories = categories;
        }

        protected override string DEFAULT_SAVE_PATH { get { return @"CommonPhaseCollaborator.xls"; } }

        public override void Process()
        {
            ReportProgress(0, "Sorting categories by project...");
            IEnumerable<ICategory> sortedCategories = SortCategoriesByProject();
            ReportProgress(15, "Finding unique contributors...");
            var uniqueContributors = FindUniqueContributors(sortedCategories);
            ReportProgress(60, "Saving to file...");
            WriteToFile(uniqueContributors);
            ReportProgress(100, "Completed processing data and saved to file.");
            MessageBox.Show(string.Format("Phase unique contributors data excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(IEnumerable<UniqueContributors> uniqueContributors)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                    // overview page
                    .CreateWorksheet("Phase unique contributors");

                ReportProgress(60, "Writing phase unique contributors page...");
                writer.CreateRow()
                    .WriteCell("Product Name", true)
                    .WriteCell("Problem / Solution contributors", true)
                    .WriteCell("Design contributors", true)
                    .WriteCell("Images contributors", true)
                    .WriteCell("Logo Design contributors", true)
                    .WriteCell("Naming contributors", true)
                    .WriteCell("Tagline contributors", true)
                    .CloseRow();

                foreach (UniqueContributors uniqueContributor in uniqueContributors)
                {
                    writer.CreateRow()
                        .WriteCell(uniqueContributor.Project, true)
                        .WriteCell(GetCellValue(uniqueContributor.Problem))
                        .WriteCell(GetCellValue(uniqueContributor.Design))
                        .WriteCell(GetCellValue(uniqueContributor.Images))
                        .WriteCell(GetCellValue(uniqueContributor.Logo))
                        .WriteCell(GetCellValue(uniqueContributor.Naming))
                        .WriteCell(GetCellValue(uniqueContributor.Tagline))
                        .CloseRow();
                }

                writer.CloseWorksheet();

                // Details page
                ReportProgress(80, "Writing details page...");
                foreach (UniqueContributors uniqueContributor in uniqueContributors)
                {
                    writer.CreateWorksheet(uniqueContributor.Project + " details")
                        .CreateRow()
                        .WriteCell("Problem / Solution contributors", true)
                        .WriteCell("Design contributors", true)
                        .WriteCell("Images contributors", true)
                        .WriteCell("Logo Design contributors", true)
                        .WriteCell("Naming contributors", true)
                        .WriteCell("Tagline contributors", true)
                        .CloseRow();

                    List<string> problemPpl = GetUniqueContributors(uniqueContributor.Problem);
                    List<string> designPpl = GetUniqueContributors(uniqueContributor.Design);
                    List<string> imagePpl = GetUniqueContributors(uniqueContributor.Images);
                    List<string> logoPpl = GetUniqueContributors(uniqueContributor.Logo);
                    List<string> namingPpl = GetUniqueContributors(uniqueContributor.Naming);
                    List<string> taglinePpl = GetUniqueContributors(uniqueContributor.Tagline);
                    int maxCount = GetMaxPpl(uniqueContributor);

                    for (int i = 0; i < maxCount; i++)
                    {
                        writer.CreateRow()
                            .WriteCell(GetPplCellValue(problemPpl, i))
                            .WriteCell(GetPplCellValue(designPpl, i))
                            .WriteCell(GetPplCellValue(imagePpl, i))
                            .WriteCell(GetPplCellValue(logoPpl, i))
                            .WriteCell(GetPplCellValue(namingPpl, i))
                            .WriteCell(GetPplCellValue(taglinePpl, i))
                            .CloseRow();
                    }
                    writer.CloseWorksheet();
                }
                writer.CloseXls();
            }
        }

        private string GetPplCellValue(List<string> ppl, int currentCount)
        {
            if (currentCount < ppl.Count)
                return ppl[currentCount];
            return string.Empty;
        }

        private int GetMaxPpl(UniqueContributors uniqueContributor)
        {
            int max = -1;

            List<string> ppl = GetUniqueContributors(uniqueContributor.Problem);
            if (ppl.Count > max) max = ppl.Count;

            ppl = GetUniqueContributors(uniqueContributor.Design);
            if (ppl.Count > max) max = ppl.Count;

            ppl = GetUniqueContributors(uniqueContributor.Images);
            if (ppl.Count > max) max = ppl.Count;

            ppl = GetUniqueContributors(uniqueContributor.Logo);
            if (ppl.Count > max) max = ppl.Count;

            ppl = GetUniqueContributors(uniqueContributor.Naming);
            if (ppl.Count > max) max = ppl.Count;

            ppl = GetUniqueContributors(uniqueContributor.Tagline);
            if (ppl.Count > max) max = ppl.Count;

            return max;
        }

        private List<string> GetUniqueContributors(ICategory phase)
        {
            if (phase == null || phase.Contributions == null) return new List<string>();

            return phase.Contributions.Select(x =>
            {
                // Assume each empty space user is a different user.
                if (string.IsNullOrWhiteSpace(x.Contributor))
                    return "User " + Guid.NewGuid();

                return x.Contributor;
            }).Distinct().OrderBy(x => x).ToList();
        }

        private string GetCellValue(ICategory phase)
        {
            if (phase == null || phase.Contributions == null) return string.Empty;

            int count = phase.Contributions.Select(x => x.Contributor).Distinct().Count();
            if (count <= 0) return string.Empty;
            return count.ToString();
        }

        private IEnumerable<UniqueContributors> FindUniqueContributors(IEnumerable<ICategory> sortedCategories)
        {
            foreach (IGrouping<string, ICategory> group in sortedCategories.GroupBy(x => x.Project))
            {
                UniqueContributors uniqueContributors = new UniqueContributors
                {
                    Project = group.Key,
                    Problem = GetCategory("problem", group),
                    Design = GetCategory("design", group),
                    Images = GetCategory("images", group),
                    Logo = GetCategory("logo", group),
                    Naming = GetCategory("naming", group),
                    Tagline = GetCategory("tagline", group)
                };
                yield return uniqueContributors;
            }
        }

        private ICategory GetCategory(string phase, IGrouping<string, ICategory> group)
        {
            return group.FirstOrDefault(x =>
            {
                if (x.Name.Length < phase.Length) return false;
                var part = x.Name.Substring(0, phase.Length);
                return string.Equals(part, phase, StringComparison.OrdinalIgnoreCase);
            });
        }

        private IEnumerable<ICategory> SortCategoriesByProject()
        {
            return this.categories.OrderBy(x => x.Project);
        }

        class UniqueContributors
        {
            public string Project { get; set; }
            public ICategory Problem { get; set; }
            public ICategory Design { get; set; }
            public ICategory Images { get; set; }
            public ICategory Logo { get; set; }
            public ICategory Naming { get; set; }
            public ICategory Tagline { get; set; }
        }
    }
}
