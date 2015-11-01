using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class PhaseCommonCollaboratorProcessor : Processor
    {
        private List<People> people;
        private List<Project> projects;
        private List<ICategory> categories;

        public PhaseCommonCollaboratorProcessor(List<Project> projects, List<ICategory> categories, List<People> people)
        {
            this.projects = projects;
            this.categories = categories;
            this.people = people;
        }

        protected override string DEFAULT_SAVE_PATH { get { return @"phaseCommonCollaborator.xls"; } }

        public override void Process()
        {
            ReportProgress(0, "Sorting projects by launch date...");
            var sortedProjects = SortProjectsByLaunchDate();
            ReportProgress(15, "Finding common phase collaborators...");
            var commonCollaborators = FindCommonCollaborators(sortedProjects);
            ReportProgress(60, "Saving to file...");
            WriteToFile(commonCollaborators);
            ReportProgress(100, "Completed processing data and saved to file.");
            MessageBox.Show(string.Format("Common phase collaborator data excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(List<Collaborators> commonCollaborators)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                // Print overview page
                .CreateWorksheet("Common Collaborators");

                ReportProgress(60, "Writing Common Collaborators page...");
                writer.CreateRow()
                    .WriteCell("Phase Name", true)
                    .WriteCell("Product Name", true)
                    .WriteCell("Idea Submitted", true)
                    .WriteCell("Days in Development", true)
                    .WriteCell("Launched", true)
                    .WriteCell("No. of past collaborators with other products", true)
                    .CloseRow();

                foreach (var project in commonCollaborators)
                {
                    var projectDetails = project.Project;
                    var submittedDate = GetSubmittedDate(projectDetails.SubmittedDate);
                    var developmentTime = GetDevelopmentTime(projectDetails.DevelopmentTime);
                    var launchedDate = GetLaunchedDate(submittedDate, developmentTime);

                    foreach (var phase in project.CommonPhaseCollaborators)
                    {
                        writer.CreateRow()
                            .WriteCell(phase.Key.Name)
                            .WriteCell(projectDetails.Name)
                            .WriteCell(submittedDate == null ? "N/A" : submittedDate.Value.ToString("dd/MM/yyyy"))
                            .WriteCell(developmentTime < 0 ? "0" : developmentTime.ToString())
                            .WriteCell(launchedDate == null ? "N/A" : launchedDate.Value.ToString("dd/MM/yyyy"))
                            .WriteCell(phase.Value.Count.ToString())
                            .CloseRow();
                    }
                }
                writer.CloseWorksheet();

                // Save the details for each project
                for (var i = 0; i < commonCollaborators.Count; i++)
                {
                    var project = commonCollaborators[i];
                    var projectName = project.Project.Name;
                    writer.CreateWorksheet(projectName);  // Create the details in a separate tab

                    var progress = 75 + (i * 25 / commonCollaborators.Count);
                    ReportProgress(progress, string.Format("Writing {0}'s details...", projectName));

                    List<List<IPeople>> allCommons = new List<List<IPeople>>();
                    List<List<IContribution>> allContributors = new List<List<IContribution>>();
                    foreach (var phase in project.CommonPhaseCollaborators)
                    {
                        var commons = phase.Value.OrderBy(x => x.Name).ToList();
                        var contributors = phase.Key.Contributions.OrderBy(x => x.Contributor).ToList();

                        allCommons.Add(commons);
                        allContributors.Add(contributors);
                    }

                    var maxRow = allCommons.Count > 0 && allContributors.Count > 0 ? Math.Max(allCommons.Max(x => x.Count), allContributors.Max(x => x.Count)) + 1 : 1;
                    // Print each contributors
                    for (var j = 0; j < maxRow; j++)
                    {
                        ReportProgress(progress, string.Format("Writing {0}'s details. Completed {1}/{2} rows", projectName, j, maxRow));
                        writer.CreateRow();

                        for (var k = 0; k < allCommons.Count; k++)
                        {
                            if (j == 0)
                            {
                                writer.WriteCell("Common Phase Collaborators", true)
                                    .WriteCell("Phase Contributors", true);
                            }
                            else
                            {
                                if (j < allCommons[k].Count)
                                    writer.WriteCell(allCommons[k][j].Name);
                                else
                                    writer.WriteCell("");

                                if (j < allContributors[k].Count)
                                    writer.WriteCell(allContributors[k][j].Contributor);
                                else
                                    writer.WriteCell("");
                            }
                        }
                        writer.CloseRow();
                    }


                    writer.CloseWorksheet();
                }

                writer.CloseXls();  // Finish book
            }
        }

        private List<Collaborators> FindCommonCollaborators(List<IProject> sortedProjects)
        {
            //List<Tuple<IProject, List<IPeople>, List<IPeople>>> collaborators = new List<Tuple<IProject, List<IPeople>, List<IPeople>>>();
            List<Collaborators> collaborators = new List<Collaborators>();
            var previous = new List<IPeople>();
            for (int i = 0; i < sortedProjects.Count; i++)
            {
                var project = sortedProjects[i];

                var projectCollaborators = this.people.Where(x => x.Contributions.Any(y => y.Project == project.Name)).Distinct().Cast<IPeople>().ToList();

                if (i == 0)
                {   // Has no collaborators with the previous
                    collaborators.Add(new Collaborators(project, new List<IPeople>(), projectCollaborators));
                    previous = new List<IPeople>(projectCollaborators);   // Initialize the common collaborators
                    continue;
                }

                ReportProgress(15 + (i * 45 / sortedProjects.Count), string.Format("Finding common collaborators for {0}. Completed {1}/{2} projects", project.Name, i, sortedProjects.Count));
                var commonCollaborators = previous.Join(projectCollaborators, c => c.Name, p => p.Name, (c, p) => c).ToList();

                previous = previous.Concat(projectCollaborators).Distinct().ToList();   // Add new people to the heap

                Collaborators myCollaborators = new Collaborators(project, commonCollaborators, projectCollaborators);
                GetCommonPhaseCollaborators(ref myCollaborators, commonCollaborators, project);
                collaborators.Add(myCollaborators);
            }

            return collaborators;
        }

        private void GetCommonPhaseCollaborators(ref Collaborators myCollaborators, List<IPeople> commonCollaborators, IProject project)
        {
            List<ICategory> phases = this.categories.Where(x => string.Equals(x.Project, project.Name)).ToList();

            foreach (ICategory phase in phases)
            {
                List<IPeople> commonPhaseCollaborators = commonCollaborators.Where(x => phase.Contributions.Any(y => string.Equals(y.Contributor, x.Name, StringComparison.OrdinalIgnoreCase))).ToList();
                myCollaborators.CommonPhaseCollaborators[phase] = commonPhaseCollaborators;
            }
        }

        class Collaborators
        {
            public IProject Project { get; set; }
            public List<IPeople> CommonProjectCollaborators { get; set; }
            public List<IPeople> ProjectCollaborators { get; set; }
            public Dictionary<ICategory, List<IPeople>> CommonPhaseCollaborators { get; set; }

            public Collaborators(IProject project, List<IPeople> commonCollaborators, List<IPeople> projectCollaborators)
            {
                Project = project;
                CommonProjectCollaborators = commonCollaborators;
                ProjectCollaborators = projectCollaborators;
                CommonPhaseCollaborators = new Dictionary<ICategory, List<IPeople>>();
            }
        }

        private List<IProject> SortProjectsByLaunchDate()
        {
            // Launch date is to be submitted date + development time as default launch date is invalid.
            return this.projects.OrderBy(x => GetLaunchedDate(x)).Cast<IProject>().ToList();
        }

        private DateTime? GetLaunchedDate(IProject project)
        {
            var submittedDate = GetSubmittedDate(project.SubmittedDate);
            var developmentTime = GetDevelopmentTime(project.DevelopmentTime);

            return GetLaunchedDate(submittedDate, developmentTime);
        }

        private DateTime? GetLaunchedDate(DateTime? submittedDate, int developmentTime)
        {
            if (submittedDate == null) return null;
            if (developmentTime < 0) return null;

            return new DateTime?(submittedDate.Value.AddDays(developmentTime));
        }

        private DateTime? GetSubmittedDate(string dateString)
        {
            return dateString.RemoveOrdinal(7, 5).TryGetDate("dddd MMMM d yyyy"); // Get submitted date as a datetime object
        }

        private int GetDevelopmentTime(string devTimeString)
        {
            int devTime;
            if (!int.TryParse(devTimeString, out devTime)) return -1;                         // Try to get the development time
            return devTime;
        }
    }
}