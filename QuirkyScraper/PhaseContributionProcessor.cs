using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    public class PhaseContributionProcessor : IProcessor
    {
        public const string BASE_FOLDER = @"D:\Users\JaG\Desktop\processedContributionResults\";

        private List<ICategory> categories;
        private string mSavePath;
        public event Action<int> ProgressChanged;

        public PhaseContributionProcessor(List<ICategory> categories)
        {
            this.categories = categories;
        }

        public string SaveFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(mSavePath))
                    mSavePath = BASE_FOLDER;

                return mSavePath;
            }

            set { mSavePath = value; }
        }

        public void Process()
        {
            if (!Directory.Exists(SaveFolderPath))
                Directory.CreateDirectory(SaveFolderPath);

            GenerateNetworks();
            MessageBox.Show("Project phases contribution network have been generated in " + SaveFolderPath);
        }

        private void GenerateNetworks()
        {
            var projects = categories.GroupBy(x => x.Project)
                .Select(x => new PhaseProject
            {
                Name = x.Key,
                People = x.SelectMany(y => y.Contributions
                    .Select(z => new PhasePerson
                    {
                        Name = z.Contributor,
                        Categories = x.Where(i => i.Contributions.Any(j => j.Contributor == z.Contributor))
                        .Select(i => new Category
                        {
                            ContributionNum = i.ContributionNum,
                            Name = i.Name,
                            Project = i.Project,
                            URL = i.URL
                        } as ICategory)
                        .ToList()
                    })).Distinct(new ContributorComparer()).ToList(),
                Phases = x.Select(y => y).ToList()
            }).ToList();

            var totalCount = projects.Count;
            var count = 0;
            var progress = 0;
            foreach (var project in projects)
            {
                ConstructAndSaveGraph(project);

                // Report project completed
                count++;
                progress = count * 100 / totalCount;
                ReportProgress(progress);
            }
        }

        private void ConstructAndSaveGraph(PhaseProject project)
        {
            var items = project.People;
            foreach (var phase in project.Phases)
            {
                if (phase.ContributionNum <= 0) continue;   // Skipping those phases without contributors

                // Build grid
                var grid = new int[items.Count, items.Count];
                for (var row = 0; row < items.Count; row++)
                {
                    for (var col = row + 1; col < items.Count; col++)
                    {
                        // To intersect, both the row and col person must have a contribution to the current phase
                        var intersects = items[row].Categories.Any(x => x.Name == phase.Name && x.URL == phase.URL && x.Project == phase.Project)
                            && items[col].Categories.Any(x => x.Name == phase.Name && x.URL == phase.URL && x.Project == phase.Project);
                        grid[row, col] = intersects ? 1 : 0;
                    }
                }

                SaveGraph(grid, items, phase);
            }
        }

        private void SaveGraph(int[,] grid, List<PhasePerson> items, ICategory phase)
        {
            var sizeX = 1000;
            var sizeY = 1000;

            var rowBlocks = (grid.GetLength(0) / sizeY) + 1;
            var colBlocks = (grid.GetLength(1) / sizeX) + 1;
            for (int i = 0; i < rowBlocks; i++)
            {
                for (int j = 0; j < colBlocks; j++)
                {
                    var dimName = i + "_" + j;
                    var filePath = Path.Combine(SaveFolderPath, (phase.Project + "_" + phase.Name + "_" + dimName).RemoveInvalidFilePathCharacters() + ".xls");
                    XmlWriter writer = Helper.GenerateXmlWriter(filePath);
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
                            writer.WriteCell(items[peopleIndex].Name, true);
                    }
                    writer.CloseRow();

                    // Populate data
                    for (var row = 0; row < sizeY; row++)
                    {
                        var rowIndex = row + startRowIndex;
                        if (rowIndex >= grid.GetLength(0)) break;

                        writer.CreateRow()
                            .WriteCell(items[rowIndex].Name, true);

                        for (var col = 0; col < sizeX; col++)
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

        private void ReportProgress(int progress)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress);
        }

        class PhaseProject
        {
            public string Name { get; set; }
            public List<PhasePerson> People { get; set; }
            public List<ICategory> Phases { get; set; }
        }

        class PhasePerson
        {
            public string Name { get; set; }
            public List<ICategory> Categories { get; set; }
        }

        private class ContributorComparer : EqualityComparer<PhasePerson>
        {
            public override bool Equals(PhasePerson x, PhasePerson y)
            {
                return x.Name == y.Name;
            }

            public override int GetHashCode(PhasePerson obj)
            {
                return obj.Name.Length + obj.Categories.Count;
            }
        }
    }
}
