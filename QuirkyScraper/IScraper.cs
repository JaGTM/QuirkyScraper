﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public interface IScraper
    {
        IEnumerable<object> Scrape();
        event Action<int, string> ProgressChanged;
    }
}
