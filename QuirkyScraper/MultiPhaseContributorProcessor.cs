using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class MultiPhaseContributorProcessor : IProcessor
    {
        public const string DEFAULT_SAVE_PATH = @"D:\Users\JaG\Desktop\multiPhaseContributor.xls";

        private List<ICategory> categories;
        private string mSavePath;

        public event Action<int, string> ProgressChanged;

        public MultiPhaseContributorProcessor(List<ICategory> categories)
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
            ReportProgress(0, "Finding multiple phases people...");
            var projects = GroupCategories();
            ReportProgress(50, "Writing results to file...");
            WriteToFile(projects);
            ReportProgress(100, "Completed finding and saving multiple phases people to file.");
            MessageBox.Show(string.Format("Multi-phase contributors excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(Dictionary<string, List<IPeople>> projects)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()

                // Print overview page
                .CreateWorksheet("Overview");

                ReportProgress(50, "Writing overview page...");
                foreach (var project in projects)
                {
                    writer.CreateRow()
                        .WriteCell(project.Key, true)
                        .WriteCell(project.Value.Count.ToString())  // Number of unique contributors with more than 1 phase in that project
                        .CloseRow();
                }

                writer.CloseWorksheet()
                    .CreateWorksheet("Details");

                var maxRows = projects.Values.Select(x => x.Count).Max() + 1;
                ReportProgress(75, "Writing names of multiple contributors page...");
                if (maxRows > 0)
                {
                    // Print each contributors
                    for (var row = 0; row < maxRows; row++)
                    {
                        ReportProgress(0, maxRows, string.Format("Writing row {0}/{1}", row, maxRows), 25, 75);
                        writer.CreateRow();
                        foreach (var project in projects)
                        {
                            if (row == 0)
                                writer.WriteCell(project.Key, true);
                            else if (project.Value.Count > row - 1)
                                writer.WriteCell(project.Value[row - 1].Name);
                            else
                                writer.WriteCell("");
                        }
                        writer.CloseRow();
                    }
                }

                writer.CloseWorksheet()
                .CloseXls();
            }
        }

        private Dictionary<string, List<IPeople>> GroupCategories()
        {
            // Sort categories by projects
            ReportProgress(0, "Grouping categories by projects");
            var groupedCategories = this.categories.GroupBy(x => x.Project).ToList();

            // Find people with more than 1 phase in each project
            var projects = new Dictionary<string, List<IPeople>>();
            var count = 0;
            foreach (var project in groupedCategories)
            {
                ReportProgress(count, groupedCategories.Count, string.Format("Finding multiple phases people in {0}. Completed: {1}/{2}", project.Key, count, groupedCategories.Count), 50);
                var projectPeople = new List<IPeople>();
                var phasesPeople = project.Select(x => x.Contributions.Select(y => y.Contributor).Distinct().ToList()).ToList();    // People in their phases
                var projectsDistinctPeople = phasesPeople.SelectMany(x => x).Distinct().ToList();                                              // All distinct people in the current project (from all phases)
                var multiplePhasesPeople = projectsDistinctPeople.Where(x => phasesPeople.Count(y => y.Any(z => z == x)) > 1).ToList();
                multiplePhasesPeople.ForEach(x =>
                {
                    var person = new People { Name = x };
                    person.AddProject(project.Key);
                    projectPeople.Add(person);
                });

                projects[project.Key] = projectPeople;
            }

            return projects;
        }

        /// <summary>
        /// Reports the progress done
        /// </summary>
        /// <param name="count">The current actual number of jobs done</param>
        /// <param name="totalCount">The total number of jobs</param>
        /// <param name="status">[Optional] Message for to display in status bar</param>
        /// <param name="percentage">[Optional] overall percentage the progress should be in</param>
        /// <param name="initialPercentage">[Optional] percentage to add to progress initially</param>
        private void ReportProgress(int count, int totalCount, string status = null, int percentage = 100, int initialPercentage = 0)
        {
            ReportProgress(initialPercentage + (count * percentage / totalCount), status);
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}