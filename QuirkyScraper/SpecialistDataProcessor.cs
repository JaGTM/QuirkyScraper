using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    internal class SpecialistDataProcessor : IProcessor
    {
        public const string DEFAULT_SAVE_PATH = @"D:\Users\JaG\Desktop\specialistData.xls";

        private List<People> specialists;
        private string mSavePath;

        public event Action<int, string> ProgressChanged;

        public SpecialistDataProcessor(List<People> specialists)
        {
            this.specialists = specialists;
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
            ReportProgress(0, "Sorting specialist by specialty...");
            var specialties = GroupSpecialty();
            ReportProgress(25, "Writing results to file...");
            WriteToFile(specialties);
            ReportProgress(100, "Completed sorting and saving specialist data to file.");
            MessageBox.Show(string.Format("Specialist data excel sheet has been created at {0}!", Savepath));
        }

        private void WriteToFile(List<Tuple<string, List<IPeople>>> specialties)
        {
            using (XmlWriter writer = Helper.GenerateXmlWriter(Savepath))
            {
                writer.StartCreateXls()

                // Print overview page
                .CreateWorksheet("Overview");

                ReportProgress(25, "Writing overview page...");
                foreach (var specialty in specialties)
                {
                    writer.CreateRow()
                        .WriteCell(specialty.Item1, true)
                        .WriteCell(specialty.Item2.Count.ToString())  // Number of unique contributors with more than 1 phase in that project
                        .CloseRow();
                }
                writer.CloseWorksheet();

                for (var i = 0; i < specialties.Count; i++)
                {
                    var specialty = specialties[i];
                    writer.CreateWorksheet(specialty.Item1);  // Create the details in a separate tab

                    var progress = 25 + (i * 75 / specialties.Count);
                    ReportProgress(progress, string.Format("Writing {0}'s specialists...", specialty.Item1));
                    // Print each contributors
                    for(var j = 0; j < specialty.Item2.Count; j++)
                    {
                        var specialist = specialty.Item2[j];
                        ReportProgress(progress, string.Format("Writing {0} of {1} specialty. Completed {2}/{3}", specialist.Name, specialty.Item1, j, specialty.Item2.Count));
                        writer.CreateRow()
                            .WriteCell(specialist.Name)
                            .CloseRow();
                    }

                    writer.CloseWorksheet();
                }

                writer.CloseXls();  // Finish book
            }
        }

        private List<Tuple<string,List<IPeople>>> GroupSpecialty()
        {
            ReportProgress(0, "Finding specialties...");
            var specialities = this.specialists.SelectMany(x => x.Skills).Distinct();
            ReportProgress(10, "Sorting specialists...");
            var specialists = specialities.Select(x => new Tuple<string, List<IPeople>>
            (
                x,
                this.specialists.Where(y => y.Skills.Contains(x)).Cast<IPeople>().ToList()
            )).ToList();

            return specialists;
        }

        private void ReportProgress(int count, int total, string status = null)
        {
            ReportProgress(count * 100 / total, status);
        }

        private void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}