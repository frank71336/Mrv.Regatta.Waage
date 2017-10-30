using Mrv.Regatta.Waage.Db.DataModels;
using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mrv.Regatta.Waage
{
    public sealed class Data
    {
        #region Singleton Pattern-Implementierung

        private static readonly Lazy<Data> lazy = new Lazy<Data>(() => new Data());

        public static Data Instance { get { return lazy.Value; } }

        private Data()
        {
        }

        #endregion

        public Frame MainContent { get; set; }
        public Einstellungen Settings { get; set; }
        public Rennen Races { get; set; }
        public List<Messung> Weightings { get; set; }
        public List<TRuderer> DbRowers { get; set; }
        public List<TVerein> DbClubs { get; set; }
        public List<TRennen> DbRaces { get; set; }
        public List<TBoote> DbBoats { get; set; }
        public List<TAbmeldung> DbCancellations { get; set; }

        public MainViewModel MainViewModel { get; set; }

        public RowersPosition RowersPosition { get; set; }

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetCurrentTime()
        {
            string timeString;
            if (MainViewModel.OverrideTime)
            {
                // manuelle Zeit nehmen
                timeString = MainViewModel.ManualTime;
            }
            else
            {
                // automatische, aktuelle Zeit nehmen
                timeString = MainViewModel.CurrentTime;
            }

            return TimeSpan.Parse(timeString);
        }
        
    }
}
