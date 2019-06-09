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

        private TimeSpan _delayTime;

        private Timer _refreshRemainingTimeTimer;

        RacesPageViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="RacesPage"/> class.
        /// </summary>
        public RacesPage()
        {
            InitializeComponent();

            GlobalData.Instance.RowersPosition = RowersPosition.Right;

            _vm = new RacesPageViewModel();
            this.DataContext = _vm;

            Refresh();

            // Timer zum Aktualisieren der grafischen verbleibenden Wiegedauer
            _refreshRemainingTimeTimer = new Timer(2 * 60 * 1000);   // 2 Minuten
            _refreshRemainingTimeTimer.Elapsed += RefreshRemainingTimeTimer_Elapsed;
            _refreshRemainingTimeTimer.Start();
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
            var day = GlobalData.Instance.Settings.ZeitstempelHeute;
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

            var rowersData = GlobalData.Instance.RowersData;
            var racesData = GlobalData.Instance.RacesData;
            // var racesConfiguration = GlobalData.Instance.RacesConfiguration; // TODO: Was ist hiermit?

            // aktueller Zeitstempel
            var day = GlobalData.Instance.Settings.ZeitstempelHeute;
            var now = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            // Alle Rennen der DB durchgehen
            var vmRaces = new System.Collections.ObjectModel.ObservableCollection<UserControls.Race>();
            foreach (var raceData in racesData)
            {
                // Neues Rennen
                var newRace = new UserControls.Race(raceData, _delayTime, GlobalData.Instance.MainViewModel.SetDelayTime);

                newRace.UpdateRemainingMinutes(now);

                // bei reduzierter Ansicht: Rennen ggf. nicht anzeigen (weil an einem anderen Tag oder schon lange vorbei oder erst in Stunden)
                // das wird erst hier gemacht, weil hier das Rennen samt Verspätung als Objekt vorhanden ist
                if (GlobalData.Instance.MainViewModel.RacesReducedView)
                {
                    // Zeit in Stunden bis zum Rennen
                    var remainingHours = (newRace.RaceDT - now).TotalHours;

                    if (remainingHours < 0)
                    {
                        // Rennen ist in der Vergangenheit
                        continue;
                    }

                    if (remainingHours > 4)
                    {
                        // Es sind noch mehr als 4 Stunden bis zum Rennen
                        continue;
                    }
                }

                // Alle Boote des Rennens
                foreach (var boatData in raceData.BoatsData)
                {
                    // Neues Boot
                    var newBoat = new UserControls.Boat(boatData);

                    // Die Ruderer des Bootes
                    var rowers = new List<UserControls.Rower>();

                    // Ruderer hinzufügen
                    foreach (var rowerData in boatData.Rowers)
                    {
                        boatBuilder.AddRower(ref rowers, raceData, rowerData, false, _delayTime);
                    }

                    // Steuermann hinzufügen
                    if (boatData.Cox != null)
                    {
                        boatBuilder.AddRower(ref rowers, raceData, boatData.Cox, false, _delayTime);
                    }

                    // Ruderer zum Boot hinzufügen
                    newBoat.Rowers = rowers;

                    // Status-Symbol des Boots entsprechend des Status der Ruderer im Boot setzen
                    boatBuilder.SetBoatStatus(raceData, boatData, newRace, newBoat);

                    // Boote zum Rennen hinzufügen
                    newRace.Boats.Add(newBoat);
                }

                // Status-Symbol des Rennens entsprechend des Status der Boote im Rennen setzen
                boatBuilder.SetRaceStatus(newRace);

                // Rennen hinzufügen
                vmRaces.Add(newRace);
            }

            _vm.Races = vmRaces;
        }

        /// <summary>
        /// Refreshes the current time.
        /// </summary>
        private void RefreshCurrentTime()
        {
            _currentTime = GlobalData.Instance.GetCurrentTime();
            _delayTime = GlobalData.Instance.GetCurrentDelay();
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
            var day = GlobalData.Instance.Settings.ZeitstempelHeute;
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

        /// <summary>
        /// Handles the Unloaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshRemainingTimeTimer.Stop();
            _refreshRemainingTimeTimer.Elapsed += RefreshRemainingTimeTimer_Elapsed;
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

                var dbRower = dbRowers.SingleOrDefault(x => x.RID == rowerId2);
                if (dbRower == null)
                {
                    TODO: Fehlermeldung bringen... !
                }

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
