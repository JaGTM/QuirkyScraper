using System;

namespace QuirkyScraper
{
    internal abstract class Processor : IProcessor
    {
        protected abstract string DEFAULT_SAVE_PATH { get; }
        protected string mSavePath;

        public event Action<int, string> ProgressChanged;

        public abstract void Process();

        public virtual string Savepath
        {
            get
            {
                if (string.IsNullOrEmpty(mSavePath))
                    mSavePath = DEFAULT_SAVE_PATH;

                return mSavePath;
            }

            set { mSavePath = value; }
        }

        protected virtual void ReportProgress(int count, int total, string status = null)
        {
            ReportProgress(count * 100 / total, status);
        }

        protected virtual void ReportProgress(int progress, string status = null)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress, status);
        }
    }
}