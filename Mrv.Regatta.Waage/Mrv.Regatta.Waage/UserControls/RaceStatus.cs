using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.UserControls
{
    public enum RaceStatus
    {
        None,
        Ok,
        OkWithProblems,
        WaitingForTimeWindow,
        WaitingInsideTimeWindow,
    }
}
