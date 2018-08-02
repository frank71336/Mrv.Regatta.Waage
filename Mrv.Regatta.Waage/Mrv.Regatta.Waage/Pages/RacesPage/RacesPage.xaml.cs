using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mrv.Regatta.Waage.Pages.RacesPage
{
    /// <summary>
    /// Interaction logic for IntroPage.xaml
    /// </summary>
    public partial class RacesPage : Page
    {
        private TimeSpan _currentTime;

        RacesPageViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="RacesPage"/> class.
        /// </summary>
        public RacesPage()
        {
            InitializeComponent();

            Data.Instance.RowersPosition = RowersPosition.Right;

            _vm = new RacesPageViewModel();
            this.DataContext = _vm;

            Refresh();

            // Timer zum Aktualisieren der grafischen verbleibenden Wiegedauer
            var refreshRemainingTimeTimer = new Timer(2 * 60 * 1000);   // 2 Minuten
            refreshRemainingTimeTimer.Elapsed += RefreshRemainingTimeTimer_Elapsed;
            refreshRemainingTimeTimer.Start();
        }

        /// <summary>
        /// Handles the Elapsed event of the RefreshRemainingTimeTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void RefreshRemainingTimeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RefreshCurrentTime(); // lokal gespeicherte Zeit aktualisieren, kann Fehler werfen!
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.Message);
                return;
            }

            // aktueller Zeitstempel
            var day = Data.Instance.Settings.ZeitstempelHeute;
            var now = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            // alle Rennen durchgehen und verbleibende Zeit aktualisieren
            foreach (var race in _vm.Races)
            {
                Tools.InvokeIfRequired(this, () =>
                {
                    race.UpdateRemainingMinutes(now);
                });
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            try
            {
                RefreshCurrentTime(); // lokal gespeicherte Zeit aktualisieren, kann Fehler werfen!
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.Message);
                return;
            }

            var boatBuilder = new BoatBuilder(_currentTime);

            var dbRowers = Data.Instance.DbRowers;
            var dbRaces = Data.Instance.DbRaces;
            var dbBoats = Data.Instance.DbBoats;
            var dbClubs = Data.Instance.DbClubs;
            var races = Data.Instance.Races;

            // aktueller Zeitstempel
            var day = Data.Instance.Settings.ZeitstempelHeute;
            var now = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            // Alle Rennen der DB durchgehen
            var vmRaces = new System.Collections.ObjectModel.ObservableCollection<UserControls.Race>();
            foreach (var dbRace in dbRaces)
            {
                // Nur ausgewählte (Leichtgewichts-)Rennen nehmen
                var race = races.Rennen1.SingleOrDefault(r => r.RennNr == dbRace.RNr);
                if (race != null)
                {
                    // Gewichte
                    var maxWeight1 = race.RennInfo.EinzelgewichtSpecified ? $"{race.RennInfo.Einzelgewicht} kg" : "-";
                    var maxWeightAverage = race.RennInfo.DurchschnittsgewichtSpecified ? $"{race.RennInfo.Durchschnittsgewicht} kg" : "-";
                    var minWeightCox = race.RennInfo.GewichtSteuermannSpecified ? $"{race.RennInfo.GewichtSteuermann} kg" : "-";

                    // Neues Rennen
                    var newRace = new UserControls.Race()
                    {
                        Id = dbRace.Index,
                        RaceNumber = dbRace.RNr,
                        RaceDT = (DateTime)dbRace.RZeit,
                        ShortName = dbRace.RKBez,
                        LongName = dbRace.RLBez,
                        Day = dbRace.RTag,
                        Time = ((DateTime)dbRace.RZeit).ToString("HH:mm"),
                        MaxWeight1 = maxWeight1,
                        MaxWeightAverage = maxWeightAverage,
                        MinWeightCox = minWeightCox,
                        DbRace = dbRace,
                        Boats = new System.Collections.ObjectModel.ObservableCollection<UserControls.Boat>()
                    };

                    newRace.UpdateRemainingMinutes(now);

                    // Boote zum Rennen hinzufügen, sortiert nach Startnummer - Achtung, Startnummer ist String, spezielle Sortierung notwendig!
                    var boats = dbBoats.Where(b => b.BRNr == dbRace.Index).OrderBy(b => b.BSNr, new BSNrComparer());

                    // Alle Boote des Rennens
                    foreach (var boat in boats)
                    {
                        // Neues Boot
                        var newBoat = new UserControls.Boat()
                        {
                            Id = boat.BID,
                            StartNumber = boat.BSNr,
                            Club = dbClubs.Single(c => c.VIDVerein == boat.BVID).VVereinsnamenKurz,
                            Status = UserControls.BoatStatus.BoatNok
                        };

                        // Die Ruderer des Bootes
                        var rowers = new List<UserControls.Rower>();

                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName1, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName2, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName3, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName4, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName5, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName6, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName7, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName8, false);
                        boatBuilder.AddRower(race, dbRace, rowers, boat.BName9, true);

                        // Ruderer zum Boot hinzufügen
                        newBoat.Rowers = rowers;

                        // Status-Symbol des Boots entsprechend des Status der Ruderer im Boot setzen
                        boatBuilder.SetBoatStatus(newRace, newBoat);

                        // Boote zum Rennen hinzufügen
                        newRace.Boats.Add(newBoat);
                    }

                    // Status-Symbol des Rennens entsprechend des Status der Boote im Rennen setzen
                    boatBuilder.SetRaceStatus(newRace);

                    // Rennen hinzufügen
                    vmRaces.Add(newRace);
                }
            }
            _vm.Races = vmRaces;
        }

        /// <summary>
        /// Refreshes the current time.
        /// </summary>
        private void RefreshCurrentTime()
        {
            _currentTime = Data.Instance.GetCurrentTime();
        }

        /// <summary>
        /// Handles the Click event of the cmdRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Handles the Click event of the cmdNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdNow_Click(object sender, RoutedEventArgs e)
        {
            ScrollToNow();
        }

        /// <summary>
        /// Scrolls to now.
        /// </summary>
        private void ScrollToNow()
        {
            try
            {
                RefreshCurrentTime(); // lokal gespeicherte Zeit aktualisieren, kann Fehler werfen!
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.Message);
                return;
            }

            // aktueller Zeitstempel
            var day = Data.Instance.Settings.ZeitstempelHeute;
            var now = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            // zuerst zum allerletzten Rennen scrollen
            var lastItem = _vm.Races.LastOrDefault();
            ScrollToItem(lastItem);

            // zuerst noch aktualisieren!
            ucRaces.GetScrollViewer().UpdateLayout();

            // dann zurück zum gewünschten Rennen scrollen, das wird dann ganz oben angezeigt

            // erstes Rennen, das später ist als jetzt
            var firstItem = _vm.Races.FirstOrDefault(r => r.RaceDT > now);
            ScrollToItem(firstItem);
        }

        /// <summary>
        /// Scrolls to item.
        /// </summary>
        /// <param name="race">The race.</param>
        private void ScrollToItem(UserControls.Race race)
        {
            if (race != null)
            {
                // es gibt ein Rennen, das später ist als jetzt

                var index = _vm.Races.IndexOf(race);

                if (index != -1)
                {
                    var frameworkElement = ucRaces.GetItemsControl().ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
                    frameworkElement.BringIntoView();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the cmdHideBoats control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdHideBoats_Click(object sender, RoutedEventArgs e)
        {
            HideBoats();
        }

        /// <summary>
        /// Hides the boats.
        /// </summary>
        private void HideBoats()
        {
            foreach (var race in _vm.Races)
            {
                foreach (var boat in race.Boats)
                {
                    if (boat.Canceled)
                    {
                        boat.Visibility = Visibility.Collapsed;
                    }

                    if (boat.Rowers.All(r => r.Status == UserControls.RowerStatus.WeightOk))
                    {
                        // alle Ruderer sind OK

                        // wenn es keinen Kommentar gibt, kann man zuklappen
                        if (string.IsNullOrEmpty(boat.Comment))
                        {
                            boat.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Page_KeyUp(object sender, KeyEventArgs e)
        {
            // Taste gedrückt - Abfrage welcher Button damit simuliert werden soll
            switch (e.Key)
            {
                case Key.F5:
                    // F5: zum aktuellen Rennen springen
                    ScrollToNow();
                    break;

                case Key.F6:
                    // F6: Boote der erledigten Rennen ausblenden
                    HideBoats();
                    break;
            }
        }


        /*
        /// <summary>
        /// Adds the rower.
        /// </summary>
        /// <param name="dbRowers">The database rowers.</param>
        /// <param name="rowers">The rowers.</param>
        /// <param name="rowerId">The rower identifier.</param>
        /// <param name="steuermann">if set to <c>true</c> [steuermann].</param>
        private void AddRower(List<UserControls.Rower> rowers, int? rowerId, bool steuermann)
        {
            if (rowerId == null)
            {
                return;
            }

            var dbRowers = Data.Instance.DbRowers;

            var rowerId2 = (int)rowerId;
            if (rowerId2 > 1)
            {
                // Id == 1 bedeutet, Platz nicht besetzt.

                var dbRower = dbRowers.Single(x => x.RID == rowerId2);

                var newRower = new UserControls.Rower()
                {
                    Id = rowerId2,
                    Name = $"{dbRower.RName}, {dbRower.RVorname} (?)",
                    Type = steuermann ? UserControls.RowerType.Cox : UserControls.RowerType.Rower,
                    Status = UserControls.RowerStatus.None
                };

                rowers.Add(newRower);
            }
        }
        */

    }
}
