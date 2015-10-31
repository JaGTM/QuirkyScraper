using System;
using System.Collections.Generic;

namespace QuirkyScraper
{
    internal class PeopleComparer : IEqualityComparer<IPeople>
    {
        public bool Strict { get; set; }

        public bool Equals(IPeople x, IPeople y)
        {
            if (!string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (Strict == true)
                return string.Equals(x.URL, y.URL, StringComparison.OrdinalIgnoreCase);
            return true;    // Name was equal and strict is false
        }

        public int GetHashCode(IPeople obj)
        {
            return obj.GetHashCode();
        }
    }
}