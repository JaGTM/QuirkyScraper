using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public interface IAmazonDetail
    {
        string URL { get; set; }
        string ReviewStars { get; set; }
        double ListPrice { get; set; }
    }
}
