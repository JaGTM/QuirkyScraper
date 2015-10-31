using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class CommonCollaboratorProcessor : Processor
    {
        protected override string DEFAULT_SAVE_PATH { get { return @"commonCollaborator.xls"; } }
        private List<People> people;
        private List<Project> projects;

        public CommonCollaboratorProcessor(List<Project> projects, List<People> people)
        {
            this.projects = projects;
            this.people = people;
        }

        public override void Process()
        {
            ReportProgress(0, "Sorting projects by launch date...");
            var sortedProjects = SortProjectsByLaunchDate();
            ReportProgress(15, "Finding common collaborators...");
            var commonCollaborators = FindCommonCollaborators(sortedProjects);
            ReportProgress(60, "Saving to file...");
            WriteToFile(commonCollaborators);
            ReportProgress(100, "Completed processing data and saved to file.");
            MessageBox.Show(string.Format("Common collaborator data excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(List<Tuple<IProject, List<IPeople>, List<IPeople>>> commonCollaborators)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()
                    // Print overview page
                .CreateWorksheet("Common Collaborators");

                ReportProgress(60, "Writing Common Collaborators page...");
                writer.CreateRow()
                    .WriteCell("Product Name", true)
                    .WriteCell("Idea Submitted", true)
                    .WriteCell("Days in Development", true)
                    .WriteCell("Launched", true)
                    .WriteCell("No. of past collaborators with other products", true)
                    .CloseRow();

                foreach (var project in commonCollaborators)
                {
                    var projectDetails = project.Item1;
                    var submittedDate = GetSubmittedDate(projectDetails.SubmittedDate);
                    var developmentTime = GetDevelopmentTime(projectDetails.DevelopmentTime);
                    var launchedDate = GetLaunchedDate(submittedDate, developmentTime);

                    writer.CreateRow()
                        .WriteCell(projectDetails.Name)
                        .WriteCell(submittedDate == null ? "N/A" : submittedDate.Value.ToString("dd/MM/yyyy"))
                        .WriteCell(developmentTime < 0 ? "0" : developmentTime.ToString())
                        .WriteCell(launchedDate == null ? "N/A" : launchedDate.Value.ToString("dd/MM/yyyy"))
                        .WriteCell(project.Item2.Count.ToString())
                        .CloseRow();
                }
                writer.CloseWorksheet();

                // Save the details for each project
                for (var i = 0; i < commonCollaborators.Count; i++)
                {
                    var project = commonCollaborators[i];
                    var projectName = project.Item1.Name;
                    writer.CreateWorksheet(projectName);  // Create the details in a separate tab

                    var progress = 75 + (i * 25 / commonCollaborators.Count);
                    ReportProgress(progress, string.Format("Writing {0}'s details...", projectName));

                    var commons = project.Item2.OrderBy(x => x.Name).ToList();
                    var contributors = project.Item3.OrderBy(x => x.Name).ToList();

                    var maxRow = Math.Max(project.Item2.Count, project.Item3.Count) + 1;
                    // Print each contributors
                    for (var j = 0; j < maxRow; j++)
                    {
                        ReportProgress(progress, string.Format("Writing {0}'s details. Completed {1}/{2} rows", projectName, j, maxRow));
                        writer.CreateRow();
                        if (j == 0)
                        {
                            writer.WriteCell("Common Collaborators", true)
                                .WriteCell("Project contributors", true);
                        }
                        else
                        {
                            if (j < commons.Count)
                                writer.WriteCell(commons[j].Name);
                            else
                                writer.WriteCell("");

                            if (j < contributors.Count)
                                writer.WriteCell(contributors[j].Name);
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

        private List<Tuple<IProject, List<IPeople>, List<IPeople>>> FindCommonCollaborators(List<IProject> sortedProjects)
        {
            List<Tuple<IProject, List<IPeople>, List<IPeople>>> collaborators = new List<Tuple<IProject, List<IPeople>, List<IPeople>>>();
            var previous = new List<IPeople>();
            for (int i = 0; i < sortedProjects.Count; i++)
            {
                var project = sortedProjects[i];

                var projectCollaborators = this.people.Where(x => x.Contributions.Any(y => y.Project == project.Name)).Distinct().Cast<IPeople>().ToList();

                if (i == 0)
                {   // Has no collaborators with the previous
                    collaborators.Add(new Tuple<IProject, List<IPeople>, List<IPeople>>(project, new List<IPeople>(), projectCollaborators));
                    previous = new List<IPeople>(projectCollaborators);   // Initialize the common collaborators
                    continue;
                }

                ReportProgress(15 + (i * 45 / sortedProjects.Count), string.Format("Finding common collaborators for {0}. Completed {1}/{2} projects", project.Name, i, sortedProjects.Count));
                var commonCollaborators = previous.Join(projectCollaborators, c => c.Name, p => p.Name, (c, p) => c).ToList();

                previous = previous.Concat(projectCollaborators).Distinct().ToList();   // Add new people to the heap

                collaborators.Add(new Tuple<IProject, List<IPeople>, List<IPeople>>(project, commonCollaborators, projectCollaborators));
            }

            return collaborators;
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