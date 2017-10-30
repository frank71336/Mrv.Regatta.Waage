using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.Pages.RacesPage
{
    class BSNrComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int val1;
            int val2;
            Int32.TryParse(x, out val1);
            Int32.TryParse(y, out val2);

            if (val1 > val2)
            {
                return 1;
            }

            if (val1 < val2)
            {
                return -1;
            }

            return 0;
        }
    }
}
