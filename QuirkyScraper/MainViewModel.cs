﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private ICommand mGenerateProductXContributors;
        private ICommand mGenerateProductInfluencers;
        private ICommand mGenerateContributorsxProducts;
        private string mStatus;
        private ICommand mScrapeFollowerFollowing;
        private ICommand mScrapeSpecialists;
        private ICommand mGenerateMultiPhaseContributor;
        private ICommand mGenerateSpecialistData;
        private ICommand mGenerateProjectDomainsCount;

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

        public ICommand GenerateProductXContributors
        {
            get { return mGenerateProductXContributors; }
            set { mGenerateProductXContributors = value; Notify(); }
        }

        public ICommand GenerateProductInfluencers
        {
            get { return mGenerateProductInfluencers; }
            set { mGenerateProductInfluencers = value; Notify(); }
        }

        public ICommand GenerateContributorsxProducts
        {
            get { return mGenerateContributorsxProducts; }
            set { mGenerateContributorsxProducts = value; Notify(); }
        }

        public ICommand ScrapeFollowerFollowing
        {
            get { return mScrapeFollowerFollowing; }
            set { mScrapeFollowerFollowing = value; Notify(); }
        }

        public ICommand ScrapeSpecialists
        {
            get { return mScrapeSpecialists; }
            set { mScrapeSpecialists = value; Notify(); }
        }

        public ICommand GenerateMultiPhaseContributor
        {
            get { return mGenerateMultiPhaseContributor; }
            set { mGenerateMultiPhaseContributor = value; Notify(); }
        }

        public ICommand GenerateSpecialistData
        {
            get { return mGenerateSpecialistData; }
            set { mGenerateSpecialistData = value; Notify(); }
        }

        public ICommand GenerateProjectDomainsCount
        {
            get { return mGenerateProjectDomainsCount; }
            set { mGenerateProjectDomainsCount = value; Notify(); }
        }

        public int Progress
        {
            get { return mProgress; }
            set { mProgress = value; Notify(); }
        }

        public string Status
        {
            get { return mStatus; }
            set { mStatus = value; Notify(); }
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
            GenerateProductXContributors = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateProductXContributors)
            };
            GenerateProductInfluencers = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateProductInfluencers)
            };
            GenerateContributorsxProducts = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateContributorsxProducts)
            };
            ScrapeFollowerFollowing = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoScrapeFollowerFollowing)
            };
            ScrapeSpecialists = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoScrapeSpecialists)
            };
            GenerateMultiPhaseContributor = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateMultiPhaseContributor)
            };
            GenerateSpecialistData = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateSpecialistData)
            };
            GenerateProjectDomainsCount = new CustomCommand
            {
                CanExecuteAction = o => !mBusy,
                ExecuteAction = o => DoBGAction(DoGenerateProjectDomainsCount)
            };
        }
        #region Actions

        private void DoGenerateProjectDomainsCount(BackgroundWorker bw)
        {
            var fp = new OpenFileDialog
            {
                Title = "Select specialist json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            var result = fp.ShowDialog();
            if (result.Value == false) return;

            List<People> specialists = null;
            try
            {
                specialists = Helper.GetJsonObjectFromFile<List<People>>(fp.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get specialist data. Exception: {0}", e);
                return;
            }

            if (specialists == null) return;

            var peoplePicker = new OpenFileDialog
            {
                Title = "Select scraped people json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            result = peoplePicker.ShowDialog();
            if (result.Value == false) return;

            List<People> people = null;
            try
            {
                people = Helper.GetJsonObjectFromFile<List<People>>(peoplePicker.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get scrapped people. Exception: {0}", e);
                return;
            }

            if (people == null) return;

            string saveFile = null;
            var saveFp = new SaveFileDialog
            {
                Title = "Select file to save to",
                Filter = "Excel 2003 | *.xls",
                FileName = "ProjectDomainsCount.xls"
            };
            result = saveFp.ShowDialog();
            if (result.HasValue == false || result.Value == false) return;  // User must specify location to save file

            saveFile = saveFp.FileName;

            IProcessor processor = new ProjectDomainsCountProcessor(specialists, people)
            {
                Savepath = saveFile
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            processor.Process();
        }

        private void DoGenerateSpecialistData(BackgroundWorker bw)
        {
            var fp = new OpenFileDialog
            {
                Title = "Select specialist json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            var result = fp.ShowDialog();
            if (result.Value == false) return;

            List<People> specialists = null;
            try
            {
                specialists = Helper.GetJsonObjectFromFile<List<People>>(fp.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get specialist data. Exception: {0}", e);
                return;
            }

            if (specialists == null) return;

            string saveFile = null;
            var saveFp = new SaveFileDialog
            {
                Title = "Select file to save to",
                Filter = "Excel 2003 | *.xls",
                FileName = "SpecialistData.xls"
            };
            result = saveFp.ShowDialog();
            if (result.HasValue == false || result.Value == false) return;  // User must specify location to save file

            saveFile = saveFp.FileName;

            IProcessor processor = new SpecialistDataProcessor(specialists)
            {
                Savepath = saveFile
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            processor.Process();
        }

        private void DoGenerateMultiPhaseContributor(BackgroundWorker bw)
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

            string saveFile = null;
            var saveFp = new SaveFileDialog
            {
                Title = "Select file to save to",
                Filter = "Excel 2003 | *.xls",
                FileName = "MultiPhaseContributors.xls"
            };
            result = saveFp.ShowDialog();
            if (result.HasValue == false || result.Value == false) return;  // User must specify location to save file

            saveFile = saveFp.FileName;

            IProcessor processor = new MultiPhaseContributorProcessor(categories)
            {
                Savepath = saveFile
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            processor.Process();
        }

        private void DoScrapeSpecialists(BackgroundWorker bw)
        {
            var sp = new SaveFileDialog
            {
                Title = "Select location to save specialists",
                Filter = "json file | *.txt",
                FileName = "scrapedSpecialists.txt"
            };
            var saveResult = sp.ShowDialog();
            if (saveResult.Value == false) return;

            IScraper scraper = new SpecialistsScraper();
            scraper.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            var results = scraper.Scrape();

            File.WriteAllText(sp.FileName, results.ToJson());
        }

        private void DoScrapeFollowerFollowing(BackgroundWorker bw)
        {
            var fp = new OpenFileDialog
            {
                Title = "Select scraped people json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            var result = fp.ShowDialog();
            if (result.Value == false) return;

            List<People> people = null;
            try
            {
                people = Helper.GetJsonObjectFromFile<List<People>>(fp.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get scrapped people. Exception: {0}", e);
                return;
            }

            var excludeFp = new OpenFileDialog
            {
                Title = "Select temp completed scraping people json",
                Filter = "json files | *.txt; *.json",
                Multiselect = false
            };
            result = excludeFp.ShowDialog();
            var tempFile = result.HasValue == false || result.Value == false ? null : excludeFp.FileName;

            var sp = new SaveFileDialog
            {
                Title = "Select location to save scraped followers followings",
                Filter = "json file | *.txt",
                FileName = "ScrapedFollowerFollowing.txt"
            };
            var saveResult = sp.ShowDialog();
            if (saveResult.Value == false) return;

            IScraper scraper = new FollowerFollowingScraper(people)
            {
                TempFilePath = tempFile
            };
            scraper.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            var results = scraper.Scrape();

            File.WriteAllText(sp.FileName, results.ToJson());
        }

        private void DoGenerateContributorsxProducts(BackgroundWorker bw)
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

            string saveFolder = null;
            var invoking = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var saveLocation = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Select location to save results to"
                };
                var saveResult = saveLocation.ShowDialog();
                if (saveResult == CommonFileDialogResult.Ok)
                {
                    saveFolder = saveLocation.FileName;
                }
            }));

            invoking.Wait();

            IProcessor processor = new GenerateContributorsXProductsProcessor(categories)
            {
                SaveFolder = saveFolder
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            processor.Process();
        }

        private void DoGenerateProductInfluencers(BackgroundWorker bw)
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

            string saveFile = null;
            var saveFp = new SaveFileDialog
            {
                Title = "Select file to save to",
                Filter = "Excel 2003 | *.xls"
            };
            result = saveFp.ShowDialog();
            if (result.Value == true)
            {
                saveFile = saveFp.FileName;
            }

            IProcessor processor = new ProductInfluencersProcessor(categories)
            {
                Savepath = saveFile
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            processor.Process();
        }

        private void DoGenerateProductXContributors(BackgroundWorker bw)
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

            string saveFile = null;
            var saveFp = new SaveFileDialog
            {
                Title = "Select file to save to",
                Filter = "Excel 2003 | *.xls"
            };
            result = saveFp.ShowDialog();
            if (result.Value == true)
            {
                saveFile = saveFp.FileName;
            }

            IProcessor processor = new ProductXContributorsProcessor(categories)
            {
                Savepath = saveFile
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
            processor.Process();
        }

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
            scraper.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
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

            string saveFolder = null;
            var invoking = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var saveLocation = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Select location to save results to"
                };
                var saveResult = saveLocation.ShowDialog();
                if (saveResult == CommonFileDialogResult.Ok)
                {
                    saveFolder = saveLocation.FileName;
                }
            }));

            invoking.Wait();

            IProcessor processor = new PhaseContributionProcessor(categories)
            {
                SaveFolderPath = saveFolder
            };
            processor.ProgressChanged += (progress, status) => bw.ReportProgress(progress, status);
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
                Status = string.Empty;
            };
            bw.ProgressChanged += (s, e) =>
            {
                Progress = e.ProgressPercentage;
                if (e.UserState != null && e.UserState.GetType() == typeof(string))
                    Status = e.UserState.ToString();
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
