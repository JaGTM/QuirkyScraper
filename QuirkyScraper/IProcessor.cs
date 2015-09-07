using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public interface IProcessor
    {
        void Process();
        event Action<int, string> ProgressChanged;
    }
}
