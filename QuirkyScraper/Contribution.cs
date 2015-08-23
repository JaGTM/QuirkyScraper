using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class Contribution : IContribution
    {
        public string Contributor { get; set; }
        public bool Selected { get; set; }
    }
}
