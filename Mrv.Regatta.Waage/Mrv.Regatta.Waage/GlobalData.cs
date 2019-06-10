using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Mrv.Regatta.Waage.DbData;

namespace Mrv.Regatta.Waage
{
    public sealed class GlobalData
    {
        #region Singleton Pattern-Implementierung

        private static readonly Lazy<GlobalData> lazy = new Lazy<GlobalData>(() => new GlobalData());

        public static GlobalData Instance { get { return lazy.Value; } }

        // Private-Konstruktor: Verhindern, dass andere Klassen Instanz erzeugen
        private GlobalData()
        {
        }

        #endregion

        public Frame MainContent { get; set; }

        // public Rennen RacesConfiguration { get; set; }

        public List<Messung> Weightings { get; set; }
        
        // Daten aus DB
        public List<RaceData> RacesData { get; set; }
        public List<EventData> EventsData { get; set; }
        public List<RowerData> RowersData { get; set; }


        //public List<TRuderer> DbRowers { get; set; }
        //public List<TVerein> DbClubs { get; set; }
        //public List<TBoote> DbBoats { get; set; }
        //public List<TAbmeldung> DbCancellations { get; set; }

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

        public TimeSpan GetCurrentDelay()
        {
            string timeString;
            if (MainViewModel.SetDelayTime)
            {
                // manuelle Zeit nehmen
                timeString = MainViewModel.DelayTime;
                return TimeSpan.Parse(timeString);
            }
            else
            {
                // keine Verzögerung
                return new TimeSpan();
                
            }
        }
        
    }
}
