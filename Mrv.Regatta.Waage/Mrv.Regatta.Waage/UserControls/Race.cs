using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Mrv.Regatta.Waage.Db.DataModels;
using System.Windows;
using System.Linq;

namespace Mrv.Regatta.Waage.UserControls
{
    public class Race : ViewModelBase.ViewModelBase
    {
        public int Id { get; set; }
        public string RaceNumber { get; set; }
        public DateTime RaceDT { get; set; }
        public string Day { get; set; }
        public string ScheduledTime { get; set; }
        public Visibility ScheduledTimeVisibility { get; set; }
        public string Time { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string MaxWeight1 { get; set; }
        public string MaxWeightAverage { get; set; }
        public string MinWeightCox{ get; set; }
        public RaceStatus Status { get; set; }

        /// <summary>
        /// Verbleibend Zeit in Minuten, aufbereitet für die grafische Anzeige (Werte 0-60)
        /// </summary>
        public int RemainingMinutes
        {
            get
            {
                return _remainingMinutes;
            }
            set
            {
                _remainingMinutes = value;
                OnPropertyChanged("RemainingMinutes");
            }
        }
        private int _remainingMinutes;

        /// <summary>
        /// Brush für die grafische Anzeige der verbleibenden Minuten
        /// </summary>
        public Brush RemainingMinutesBrush
        {
            get
            {
                return _remainingMinutesBrush;
            }
            set
            {
                _remainingMinutesBrush = value;
                OnPropertyChanged("RemainingMinutesBrush");
            }
        }
        private Brush _remainingMinutesBrush;

        public TRennen DbRace { get; set; }

        public ObservableCollection<Boat> Boats { get; set; }

        /// <summary>
        /// Updates the remaining minutes.
        /// </summary>
        public void UpdateRemainingMinutes(DateTime now)
        {
            // Zeit in Minuten bis zum Rennen
            var remainingMinutes = (int)(RaceDT - now).TotalMinutes;

            // Werte > 0: Rennen ist in der Zukunft
            // Werte < 0: Rennen war in der Vergangenheit

            if (remainingMinutes < 60)
            {
                // Rennen ist in weniger als einer Stunde oder es ist schon vorbei (remainingMinutes < 0)
                RemainingMinutes = 0;

                // Wenn der Wert 0 ist, dann ist die Farbe egal
            }
            else if (remainingMinutes < 120)
            {
                // 60 - 120 Minuten bis zum Rennen
                // genau im Fenster zum Wiegen

                RemainingMinutes = remainingMinutes - 60;

                if (RemainingMinutes > 30)
                {
                    // noch genügen Zeit => grün
                    RemainingMinutesBrush = new LinearGradientBrush(Color.FromRgb(35, 135, 35), Color.FromRgb(35, 255, 35), new Point(0, 0), new Point(1, 0));
                }
                else if (RemainingMinutes > 10)
                {
                    // 10 - 30 Minuten => gelb
                    RemainingMinutesBrush = new LinearGradientBrush(Color.FromRgb(135, 135, 35), Color.FromRgb(200, 200, 35), new Point(0, 0), new Point(1, 0));
                }
                else
                {
                    // weniger als 10 Minuten => rot
                    RemainingMinutesBrush = new LinearGradientBrush(Color.FromRgb(135, 35, 35), Color.FromRgb(255, 35, 35), new Point(0, 0), new Point(1, 0));
                }
            }
            else
            {
                // noch länger als 2 Stunden hin
                RemainingMinutes = 60;

                // Kinderrennen?
                var racesConfiguration = Data.Instance.RacesConfiguration;
                var race = racesConfiguration.Rennen1.SingleOrDefault(r => r.RennNr == DbRace.RNr);
                if (race == null)
                {
                    Tools.LogError("XML-Rennen zu DB-Rennen nicht gefunden! RennNr", DbRace.RNr);
                    return;
                }

                if (race.RennInfo.Kinderrennen)
                {
                    // Kinderennen => grün
                    RemainingMinutesBrush = new LinearGradientBrush(Color.FromRgb(35, 135, 35), Color.FromRgb(35, 255, 35), new Point(0, 0), new Point(1, 0));
                }
                else
                {
                    // kein Kinderrennen => blau
                    RemainingMinutesBrush = new LinearGradientBrush(Color.FromRgb(35, 35, 135), Color.FromRgb(35, 35, 255), new Point(0, 0), new Point(1, 0));
                }
            }
        }
    }
}
