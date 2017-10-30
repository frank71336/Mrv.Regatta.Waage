using Mrv.Regatta.Waage.Db.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.UserControls
{
    public class Race
    {
        public int Id { get; set; }
        public string RaceNumber { get; set; }
        public DateTime RaceDT { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string CountdownTime { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string MaxWeight1 { get; set; }
        public string MaxWeightAverage { get; set; }
        public string MinWeightCox{ get; set; }
        public RaceStatus Status { get; set; }

        public TRennen DbRace { get; set; }

        public ObservableCollection<Boat> Boats { get; set; }
    }
}
