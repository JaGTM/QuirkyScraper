using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuirkyScraper
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ICommand mScrapeParticipants;
        private bool mBusy;
        private ICommand mGenerateProductContribution;
        private ICommand mGeneratePhaseContribution;
        private ICommand mScrapePeople;
        private int mProgress;

        private void Notify([CallerMemberName]string name = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #region Properties
        public ICommand ScrapeParticipants
        {
            get { return mScrapeParticipants; }
            set { mScrapeParticipants = value; Notify(); }
        }

        public ICommand GenerateProductContribution
        {
            get { return mGenerateProductContribution; }
            set { mGenerateProductContribution = value; Notify(); }
        }

        public ICommand GeneratePhaseContribution
        {
            get { return mGeneratePhaseContribution; }
            set { mGeneratePhaseContribution = value; Notify(); }
        }

        public ICommand ScrapePeople
        {
            get { return mScrapePeople; }
            set { mScrapePeople = value; Notify(); }
        }

        public int Progress
        {
            get { return mProgress; }
            set { mProgress = value; Notify(); }
        }
        #endregion

        public MainViewModel()
        {
            BindCommands();
        }

        private void BindCommands()
        {
            ScrapeParticipants = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoScrapeParticipants)
            };
            GenerateProductContribution = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateProductContribution)
            };
            GeneratePhaseContribution = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGeneratePhaseContribution)
            };
            ScrapePeople = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoScrapePeople)
            };
        }
        #region Actions

        private void DoScrapePeople(BackgroundWorker bw)
        {
            var fp = new OpenFileDialog
            {
                Title = "Select processed category json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            var result = fp.ShowDialog();
            if (result.Value == false) return;

            List<ICategory> categories = null;
            try
            {
                categories = ParticipantScraper.GetExistingProcessCategories(fp.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start scrape people. Exception: {0}", e);
                return;
            }

            if (categories == null) return;

            var excludeFp = new OpenFileDialog
            {
                Title = "Select excluding projects json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            result = excludeFp.ShowDialog();
            if (result.Value == true)
            {
                var excludeProject = Helper.GetJsonObjectFromFile<List<Project>>(excludeFp.FileName);
                categories = categories.Where(x => !excludeProject.Any(y => string.Equals(x.Project, y.Name))).ToList();
            }

            IScraper scraper = new PeopleScraper(categories);
            scraper.ProgressChanged += progress => bw.ReportProgress(progress);
            var results = scraper.Scrape();

            var output = @"C:\Users\JaG\Desktop\peopleScraped.txt";
            File.WriteAllText(output, results.ToJson());
        }

        private void DoGeneratePhaseContribution(BackgroundWorker bw)
        {
            var fp = new OpenFileDialog
            {
                Title = "Select processed category json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            var result = fp.ShowDialog();
            if (result.Value == false) return;

            List<ICategory> categories = null;
            try
            {
                categories = ParticipantScraper.GetExistingProcessCategories(fp.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to generate production contribution graph. Exception: {0}", e);
                return;
            }

            if (categories == null) return;

            var excludeFp = new OpenFileDialog
            {
                Title = "Select excluding projects json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            result = excludeFp.ShowDialog();
            if (result.Value == true)
            {
                var excludeProject = Helper.GetJsonObjectFromFile<List<Project>>(excludeFp.FileName);
                categories = categories.Where(x => !excludeProject.Any(y => string.Equals(x.Project, y.Name))).ToList();
            }

            IProcessor processor = new PhaseContributionProcessor(categories);
            processor.Process();
        }

        private void DoGenerateProductContribution(BackgroundWorker bw)
        {
            var fp = new OpenFileDialog
            {
                Title = "Select processed category json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            var result = fp.ShowDialog();
            if (result.Value == false) return;

            List<ICategory> categories = null;
            try
            {
                categories = ParticipantScraper.GetExistingProcessCategories(fp.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to generate production contribution graph. Exception: {0}", e);
                return;
            }

            if (categories == null) return;

            var excludeFp = new OpenFileDialog
            {
                Title = "Select excluding projects json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            result = excludeFp.ShowDialog();
            if (result.Value == true)
            {
                var excludeProject = Helper.GetJsonObjectFromFile<List<Project>>(excludeFp.FileName);
                categories = categories.Where(x => !excludeProject.Any(y => string.Equals(x.Project, y.Name))).ToList();
            }

            IProcessor processor = new ProductContributionProcessor(categories);
            processor.Process();
        }

        /// <summary>
        /// Frame to hold the actual work on a background thread. Should only be called by a command
        /// </summary>
        /// <param name="action">Actual work to be done</param>
        private void DoBGAction(Action<BackgroundWorker> action)
        {
            if (mBusy == true) return;
            mBusy = true;

            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.DoWork += (s, e) => action(bw);
            bw.RunWorkerCompleted += (s, e) =>
            {
                mBusy = false;
                Progress = 0;
            };
            bw.ProgressChanged += (s, e) =>
            {
                Progress = e.ProgressPercentage;
            };

            bw.RunWorkerAsync();
        }

        private void DoScrapeParticipants(BackgroundWorker bw)
        {
            List<string> projectUrls = null;
            try
            {
                var fp = new OpenFileDialog
                {
                    Title = "Select project details json",
                    Filter = "json files | *.txt; *.json",
                    Multiselect = false
                };
                var result = fp.ShowDialog();
                if (result.Value == false) return;

                var projects = Helper.GetJsonObjectFromFile<List<Project>>(fp.FileName);
                projectUrls = projects.Select(x => x.URL).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start scrape participants. Exception: {0}", e);
            }

            if (projectUrls == null || projectUrls.Count == 0) return;

            IScraper scraper = new ParticipantScraper(projectUrls);
            var results = scraper.Scrape();

            var output = @"C:\Users\JaG\Desktop\myresults.txt";
            File.WriteAllText(output, results.ToJson());
        }
        #endregion
    }
}
