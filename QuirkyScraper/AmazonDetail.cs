using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuirkyScraper
{
    public class AmazonDetail: IAmazonDetail
    {
        public string URL { get; set; }
        public string ReviewStars { get; set; }
        public double ListPrice { get; set; }
    }
}
