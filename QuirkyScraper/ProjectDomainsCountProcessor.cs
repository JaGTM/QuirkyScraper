using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class ProjectDomainsCountProcessor : Processor
    {
        protected override string DEFAULT_SAVE_PATH { get { return "ProjectDomainsCount.xls"; } }

        private List<People> people;
        private List<People> specialists;

        public ProjectDomainsCountProcessor(List<People> specialists, List<People> people)
        {
            this.specialists = specialists;
            this.people = people;
        }

        public override void Process()
        {
            ReportProgress(0, "Counting domains for each project...");
            var domainsAndCounts = CountDomains();
            ReportProgress(75, "Writing results to file...");
            WriteToFile(domainsAndCounts);
            ReportProgress(100, "Completed counting and saving project domains count to file.");
            MessageBox.Show(string.Format("Project domains count excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(Dictionary<string, List<string>> domainsAndCounts)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                .CreateWorksheet("Knowledge Domain");


                ReportProgress(75, "Writing Knowledge Domain page...");
                ReportProgress(75, "Writing Header...");
                writer.CreateRow()
                    .WriteCell("Name of Product", true)
                    .WriteCell("Number of Knowledge Domain Involved", true)
                    .CloseRow();

                ReportProgress(75, "Writing values...");
                var i = 0;
                var keys = domainsAndCounts.Keys.OrderBy(x => x).ToList();
                foreach (var key in keys)
                {
                    var count = domainsAndCounts[key].Count;
                    writer.CreateRow()
                        .WriteCell(key)
                        .WriteCell(count.ToString())
                        .CloseRow();
                    ReportProgress(75 + (++i * 5 / domainsAndCounts.Keys.Count), string.Format("Completed writing {0}/{1} project...", i, domainsAndCounts.Keys.Count));
                }
                writer.CloseWorksheet();

                var maxRows = domainsAndCounts.Values.Select(x => x.Count).Max() + 1;
                ReportProgress(80, "Writing details...");
                writer.CreateWorksheet("Details");
                for (var row = 0; row < keys.Count; row++)
                {
                    ReportProgress(80 + (row * 20 / maxRows), string.Format("Writing row {0}/{1}", row, maxRows));
                    writer.CreateRow();
                    foreach (var key in keys)
                    {
                        var domains = domainsAndCounts[key].OrderBy(x => x).ToList();
                        if (row == 0)
                            writer.WriteCell(key, true);
                        else if (domains.Count > row - 1)
                            writer.WriteCell(domains[row - 1]);
                        else
                            writer.WriteCell("");
                    }
                    writer.CloseRow();
                }
                writer.CloseWorksheet()
                    .CloseXls();  // Finish book
            }
        }

        public Dictionary<string, List<string>> CountDomains()
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
                    Project = y.Project
                };
            })).GroupBy(x => x.Project)
            .Select(x => new
            {
                Project = x.Key,
                Domains = x.SelectMany(y => y.Person.Skills).Distinct().ToList()    // All of each persons domain will be added
            })
            .ToDictionary(x => x.Project, x => x.Domains);

            return items;
        }
    }
}