using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
   public interface IContribution
    {
       string Contributor { get; set; }
       bool Selected { get; set; }
    }
}
