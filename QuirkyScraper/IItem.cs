using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuirkyScraper
{
    public interface IItem
    {
        bool Equals(IItem item2);
    }
}
