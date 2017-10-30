using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.Pages.IntroPage
{
    public class IntroPageViewModel : ViewModelBase.ViewModelBase
    {
        public string Version { get; set; }
        public int RacesCount { get; set; }
        public int BoatsCount { get; set; }
        public int RowersCount { get; set; }
        public int ClubsCount { get; set; }
    }
}
