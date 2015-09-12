using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class ProjectPhaseDomainsCountProcessor : Processor
    {
        protected override string DEFAULT_SAVE_PATH { get; } = "ProjectPhaseDomainsCount.xls";

        private List<People> people;
        private List<People> specialists;

        public ProjectPhaseDomainsCountProcessor(List<People> specialists, List<People> people)
        {
            this.specialists = specialists;
            this.people = people;
        }

        public override void Process()
        {
            ReportProgress(0, "Counting domains for each project...");
            var projectProcessor = new ProjectDomainsCountProcessor(this.specialists, this.people);
            var projectDomainsAndCounts = projectProcessor.CountDomains();

            ReportProgress(25, "Counting domains for each phase...");
            var phaseProcessor = new PhaseDomainsCountProcessor(this.specialists, this.people);
            var phaseDomainsAndCounts = phaseProcessor.CountDomains();
            
            ReportProgress(50, "Writing results to file...");
            WriteToFile(projectDomainsAndCounts, phaseDomainsAndCounts);
            ReportProgress(100, "Completed counting and saving project and phase domains count to file.");
            MessageBox.Show(string.Format("Project and phase domains count excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(Dictionary<string, List<string>> projectDomainsAndCounts, Dictionary<ICategory, List<string>> phaseDomainsAndCounts)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                .CreateWorksheet("Knowledge Domain");

                var phasesHeaders = phaseDomainsAndCounts.Keys.Select(x => x.Name).Distinct().OrderBy(x => x).ToList();

                ReportProgress(50, "Writing Knowledge Domain page...");
                ReportProgress(50, "Writing Header...");
                writer.CreateRow()
                    .WriteCell("Name of Product", true)
                    .WriteCell("Total number of Knowledge Domain Involved", true);
                foreach (var phaseHeader in phasesHeaders)
                    writer.WriteCell("Number in " + phaseHeader);
                writer.CloseRow();

                ReportProgress(50, "Writing values...");
                var i = 0;
                var projectKeys = projectDomainsAndCounts.Keys.OrderBy(x => x).ToList();
                var phaseKeys = phaseDomainsAndCounts.Keys.ToList();
                foreach (var projectKey in projectKeys)
                {
                    var projectPhasesKeys = phaseKeys.Where(x => x.Project == projectKey).ToList();

                    var projectCount = projectDomainsAndCounts[projectKey].Count;
                    writer.CreateRow()
                        .WriteCell(projectKey)
                        .WriteCell(projectCount);

                    foreach(var phaseHeader in phasesHeaders)
                    {
                        var projectPhaseKey = projectPhasesKeys.FirstOrDefault(x => x.Name == phaseHeader);
                        if (projectPhaseKey == null)
                            writer.WriteCell("");
                        else
                        {
                            var phaseCount = phaseDomainsAndCounts[projectPhaseKey].Count;
                            writer.WriteCell(phaseCount);
                        }
                    }

                    writer.CloseRow();
                    ReportProgress(50 + (++i * 10 / projectDomainsAndCounts.Keys.Count), string.Format("Completed writing {0}/{1} project...", i, projectDomainsAndCounts.Keys.Count));
                }
                writer.CloseWorksheet();
                
                var phaseProjects = phaseDomainsAndCounts.Keys.GroupBy(x => x.Project).ToList();
                for (var projCount = 0; projCount < phaseProjects.Count; projCount++)
                {
                    var project = phaseProjects[projCount];

                    var currentProjectProgress = 60 + projCount * 40 / phaseProjects.Count;
                    ReportProgress(currentProjectProgress, string.Format("Writing details for {0}...", project.Key));
                    writer.CreateWorksheet(project.Key + " Details");

                    var phaseMaxRows = phaseDomainsAndCounts.Where(x => project.Any(y => x.Key == y)).Select(x => x.Value.Count).Max();
                    var projectMaxRows = projectDomainsAndCounts[project.Key].Count;
                    var maxRows = Math.Max(phaseMaxRows, projectMaxRows) + 1;
                    for (var row = 0; row < maxRows; row++)
                    {
                        ReportProgress(currentProjectProgress, string.Format("Writing row {0}/{1} for {2}", row, maxRows, project.Key));
                        writer.CreateRow();

                        // Write project stuff first
                        var projectDomains = projectDomainsAndCounts[project.Key].OrderBy(x => x).ToList();
                        if (row == 0)
                            writer.WriteCell("Distinct Domains for " + project.Key, true);
                        else if (projectDomains.Count > row - 1)
                            writer.WriteCell(projectDomains[row - 1]);
                        else
                            writer.WriteCell("");

                        // Next write phases stuff
                        var phases = project.OrderBy(x => x.Name).ToList();
                        foreach (var phaseKey in phases)
                        {
                            var phaseDomains = phaseDomainsAndCounts[phaseKey].OrderBy(x => x).ToList();
                            if (row == 0)
                                writer.WriteCell(phaseKey.Name, true);
                            else if (phaseDomains.Count > row - 1)
                                writer.WriteCell(phaseDomains[row - 1]);
                            else
                                writer.WriteCell("");
                        }
                        writer.CloseRow();
                    }
                    writer.CloseWorksheet();
                }

                writer.CloseXls();  // Finish book
            }
        }
    }
}