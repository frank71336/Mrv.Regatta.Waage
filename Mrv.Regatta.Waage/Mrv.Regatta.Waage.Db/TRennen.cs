using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.Db.DataModels
{
    public partial class TRennen
    {
        public override string ToString()
        {
            return $"{RTag} {((DateTime)RZeit).TimeOfDay} [{RNr}] - {RKBez}";
        }
    }
}
