using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class PhaseDomainsCountProcessor : Processor
    {
        protected override string DEFAULT_SAVE_PATH { get; } = "PhaseDomainsCount.xls";

        private List<People> people;
        private List<People> specialists;

        public PhaseDomainsCountProcessor(List<People> specialists, List<People> people)
        {
            this.specialists = specialists;
            this.people = people;
        }

        public override void Process()
        {
            ReportProgress(0, "Counting domains for each phase...");
            var domainsAndCounts = CountDomains();
            ReportProgress(75, "Writing results to file...");
            WriteToFile(domainsAndCounts);
            ReportProgress(100, "Completed counting and saving project domains count to file.");
            MessageBox.Show(string.Format("Phase domains count excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(Dictionary<ICategory, List<string>> domainsAndCounts)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                .CreateWorksheet("Knowledge Domain");


                ReportProgress(75, "Writing Knowledge Domain page...");
                ReportProgress(75, "Writing Header...");
                writer.CreateRow()
                    .WriteCell("Name of Phase", true)
                    .WriteCell("Name of Product", true)
                    .WriteCell("Number of Knowledge Domain Involved", true)
                    .CloseRow();

                ReportProgress(75, "Writing values...");
                var i = 0;
                var keys = domainsAndCounts.Keys.ToList();
                foreach (var key in keys)
                {
                    var count = domainsAndCounts[key].Count;
                    writer.CreateRow()
                        .WriteCell(key.Name)
                        .WriteCell(key.Project)
                        .WriteCell(count.ToString())
                        .CloseRow();
                    ReportProgress(75 + (++i * 5 / domainsAndCounts.Keys.Count), string.Format("Completed writing {0}/{1} project...", i, domainsAndCounts.Keys.Count));
                }
                writer.CloseWorksheet();

                var projects = domainsAndCounts.Keys.GroupBy(x => x.Project).ToList();
                for(var projCount = 0; projCount < projects.Count; projCount++)
                {
                    var project = projects[projCount];

                    var currentProjectProgress = 80 + projCount * 20 / projects.Count;
                    ReportProgress(80 + projCount * 20/ projects.Count, string.Format("Writing details for {0}...", project.Key));
                    writer.CreateWorksheet(project.Key + " Details");

                    var maxRows = domainsAndCounts.Where(x => project.Any(y => x.Key == y)).Select(x => x.Value.Count).Max() + 1;
                    for(var row = 0; row < maxRows; row++)
                    {
                        ReportProgress(currentProjectProgress, string.Format("Writing row {0}/{1} for {2}", row, maxRows, project.Key));
                        writer.CreateRow();
                        foreach (var phaseKey in project)
                        {
                            var domains = domainsAndCounts[phaseKey].OrderBy(x => x).ToList();
                            if (row == 0)
                                writer.WriteCell(phaseKey.Name, true);
                            else if (domains.Count > row - 1)
                                writer.WriteCell(domains[row - 1]);
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

        private Dictionary<ICategory, List<string>> CountDomains()
        {
            var items = this.people.SelectMany(x => x.Contributions.Select(y =>
            {
                var person = new People
                {
                    Name = x.Name,
                    URL = x.URL
                };
                var expert = this.specialists.FirstOrDefault(z => z.Name == x.Name && z.URL == x.URL);
                if (expert != null)
                {
                    var domains = expert.Skills.ToList();
                    domains.ForEach(z => person.AddSkill(z));
                }

                return new
                {
                    Person = person,
                    Phase = y,
                    Project = y.Project
                };
            })).GroupBy(x => x.Project)
            .Select(x => new
            {
                Project = x.Key,
                Phases = x.GroupBy(y => y.Phase.Name)
                .Select(y => new
                {
                    Phase = y.First().Phase,
                    Domains = y.SelectMany(z => z.Person.Skills).Distinct().ToList()    // All of each persons domain will be added
                }).ToList(),

            }).SelectMany(x => x.Phases.Select(y => new
            {
                Phase = y.Phase,
                Domains = y.Domains
            }))
            .ToDictionary(x => x.Phase, x => x.Domains);

            return items;
        }
    }
}