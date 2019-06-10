using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Mrv.Regatta.Waage.Windows;
using ViewModelBase.CollectionExtensions;
using System.Timers;
using Mrv.Regatta.Waage.DbData;

namespace Mrv.Regatta.Waage.Pages.RowerPage
{
    /// <summary>
    /// Interaction logic for RowerPage.xaml
    /// </summary>
    public partial class RowerPage : Page
    {
        private TimeSpan _currentTime;

        private TimeSpan _delayTime;

        private Timer _refreshRemainingTimeTimer;

        private RowerPageViewModel _vm;

        private int _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowerPage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public RowerPage(int id)
        {
            InitializeComponent();

            GlobalData.Instance.RowersPosition = RowersPosition.Left;

            _id = id;

            _vm = new RowerPageViewModel();
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
            var day = Properties.Settings.Default.Today;
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

            #region Infos zum Ruderer

            var dbRower = rowersData.SingleOrDefault(x => x.Id == _id);

            _vm.Name = $"{dbRower.LastName}, {dbRower.FirstName}";
            _vm.Gender = dbRower.Gender;
            _vm.YearOfBirth = dbRower.DateOfBirth.Year.ToString();
            _vm.Club = dbRower.ClubTitleLong;

            #endregion

            #region bisherige Gewichtsmessungen

            // Deutsche Bezeichnung für die Wochentage
            var culture = new System.Globalization.CultureInfo("de-DE");

            // Alle bisherigen Gewichtsmessungen
            _vm.Weightings = GlobalData.Instance.Weightings.Where(x => x.Id == _id).OrderByDescending(x => x.Zeitstempel).Select(x => new RowerPageViewModel.Weighting()
            {
                Mass = x.Gewicht.ToString(),
                Date = culture.DateTimeFormat.GetDayName(x.Zeitstempel.DayOfWeek),
                Time = $"{x.Zeitstempel:HH:mm:ss}"

            }).ToObservableCollection();

            #endregion

            #region Rennen

            // alle Rennen, an denen unser Ruderer teilnimmt
            var racesDataThisRower = new List<RaceData>();
            foreach(var raceData in racesData)
            {
                // schauen ob es im aktuell betrachteten Rennen ein Boot gibt, in dem unser Ruderer sitzt
                // (entweder als Steuermann oder als Teil der Mannschaft)
                if (raceData.HasRower(_id))
                {
                    racesDataThisRower.Add(raceData);
                }
            }

            // aktueller Zeitstempel
            var day = Properties.Settings.Default.Today;
            var now = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            // Alle Rennen mit unserem Ruderer durchgehen
            _vm.Races = new System.Collections.ObjectModel.ObservableCollection<UserControls.Race>();
            foreach (var raceData in racesDataThisRower)
            {
                // Gewichte
                var maxWeight1 = raceData.MaxSingleWeight.ToDisplayString();
                var maxWeightAverage = raceData.MaxAverageWeight.ToDisplayString();
                var minWeightCox = raceData.MinCoxWeight.ToDisplayString();

                // Neues Rennen
                var newRace = new UserControls.Race(raceData, _delayTime, GlobalData.Instance.MainViewModel.SetDelayTime);

                // Boote nur mit diesem Ruderer drin zum Rennen hinzufügen
                // (da sollte eigentlich nur genau 1 Boot rauskommen)
                var boatsData = raceData.BoatsData.Where(b => b.HasRower(_id));

                // Alle Boote des Rennens
                foreach (var boatData in boatsData)
                {
                    // Neues Boot
                    var newBoat = new UserControls.Boat(boatData);

                    // Die Ruderer des Bootes
                    var rowers = new List<UserControls.Rower>();

                    // Ruderer hinzufügen
                    foreach(var rowerData in boatData.Rowers)
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

                newRace.UpdateRemainingMinutes(now);

                // Rennen hinzufügen
                _vm.Races.Add(newRace);
            }

            _vm.Races = _vm.Races.OrderBy(r => r.RaceDT).ToObservableCollection();

            #endregion

            #region Kommentar

            // das nächste Rennen des Ruderers ermitteln

            // alle Rennen des Ruderers (nur die, die in der View auch angezeigt werden)
            var races1 = racesData.Where(rd => (_vm.Races?.Any(r => r.RaceNumber == rd.RaceNumber) == true)).OrderBy(r => r.DateTime).ToList();

            // alle Rennen, die noch kommen
            var futureRaces = races1.Where(r => (r.DateTime + _delayTime)  > now);

            var nextRace = futureRaces.FirstOrDefault();

            if (nextRace == null)
            {
                // es gibt keine weiteren Rennen mehr
                _vm.Comment = "Keine weiteren Rennen mehr.";
                _vm.CommentBrush = new SolidColorBrush(Colors.LightSkyBlue);
            }
            else
            {
                var timeDiff = nextRace.DateTime + _delayTime - now;

                if (timeDiff.TotalHours > 2)
                {
                    // es ist noch länger als 2 Stunden bis zum nächsten Rennen

                    // jetzt kommt es noch drauf an, ob es ein Kinderrennen ist
                    if (nextRace.IsChildrenRace)
                    {
                        // es ist ein Kinderrennen, dann ist das Wiegen auch so früh schon möglich
                        _vm.CommentBrush = new SolidColorBrush(Colors.LightYellow);
                    }
                    else
                    {
                        // es ist kein Kinderrennen, dann darf er auch nicht Wiegen
                        _vm.CommentBrush = new SolidColorBrush(Colors.Red);
                    }

                }
                else
                {
                    // es ist weniger als 2 Stunden bis zum nächsten Rennen
                    _vm.CommentBrush = new SolidColorBrush(Colors.LightGreen);
                }

                _vm.Comment = $"Nächstes Rennen ({nextRace.RaceNumber}): {timeDiff.Hours} h {timeDiff.Minutes} min.";
            }

            #endregion
        }

        /// <summary>
        /// Lässt nur Zahlen (mit Komma) in der Textbox zu.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the event data.</param>
        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        /// <summary>
        /// Determines whether string is numeric.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if [is text numeric] [the specified string]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"[0-9,\.]");
            return reg.IsMatch(str);
        }

        /// <summary>
        /// Handles the KeyDown event of the cmdNewWeighting control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void cmdNewWeighting_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return) || (e.Key == Key.Enter))
            {
                // Zeichenkette bereinigen
                var valueStr = txtNewWeighting.Text.Trim().Replace(".", ",");

                if (string.IsNullOrEmpty(valueStr))
                {
                    Tools.ShowError("Keine Eingabe!");
                    return;
                }

                try
                {
                    // Zeichenkette in Zahl wandeln
                    var value = Convert.ToSingle(valueStr);

                    if (value < 25)
                    {
                        Tools.ShowError("Unplausible Eingabe!");
                        return;
                    }

                    // Daten Ruderer
                    var rowerData = GlobalData.Instance.RowersData.Single(rd => rd.Id == _id);

                    // Zeitstempel
                    var day = Properties.Settings.Default.Today;

                    // "RefreshCurrentTime" kann Exception werfen, die wird aber weiter unten schon abgefangen
                    RefreshCurrentTime(); // lokal gespeicherte Zeit aktualisieren
                    var dt = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

                    // Speicherort
                    var fileName = $"{dt.Year}-{dt.Month:00}-{dt.Day:00} {dt.Hour:00}-{dt.Minute:00}-{dt.Second:00} {Guid.NewGuid().ToString()}.xml";
                    var path = Properties.Settings.Default.WeighingsPath;
                    var filePath = Path.Combine(path, fileName);

                    // Daten der Messung
                    var messung = new Messung();
                    messung.Id = _id;
                    messung.Name = $"{rowerData.LastName}, {rowerData.FirstName} ({rowerData.ClubTitleShort})";
                    messung.Zeitstempel = dt;
                    messung.Gewicht = value;

                    // Messung speichern
                    messung.Save(filePath);

                    // Messungen im Speicher aktualisieren
                    Tools.ReadWeightings();

                    // Ansicht aktualisieren
                    Refresh();

                    // Textbox löschen
                    txtNewWeighting.Text = "";

                    // Log-Eintrag
                    using (StreamWriter streamWriter = File.AppendText(Properties.Settings.Default.WeighingsLogFile))
                    {
                        var logName = FormatString(_vm.Name, 30);
                        var logClub = FormatString(_vm.Club, 25);
                        streamWriter.WriteLine($"{logName} | {_vm.YearOfBirth} | {logClub} | {dt} | {value}");
                    }

                    #region Bestätigungsfenster anzeigen

                    // Alle Rennen mit diesem Ruderer
                    var races = _vm.Races.Where(rc => (rc.Boats?.Any(bt => bt.Rowers.Any(rr => rr.Id == _id)) == true));

                    // nur Rennen in der Zukunft sind von Bedeutung
                    races = races.Where(r => r.RaceDT > dt);

                    // Das nächste Rennen
                    var nextRace = races.FirstOrDefault();
                    
                    ConfirmWindow.Status status;

                    if (nextRace == null)
                    {
                        // Es gibt gar kein nächstes Rennen mehr
                        // => Unklar, warum der überhaupt zum Wiegen kommt
                        status = ConfirmWindow.Status.Unknown;
                    }
                    else
                    {
                        // Es gibt ein nächstes Rennen, das ist die Grundlage für die weitere Bewertung

                        // Das Boot des nächsten Rennens
                        var boat = nextRace.Boats.SingleOrDefault(b => (b.Rowers?.Any(r => r.Id == _id) == true));
                        if (boat == null)
                        {
                            Tools.LogError("Boot mit diesem Ruderer im nächsten Rennen nicht gefunden oder mehrere Boote gefunden: RID", _id, "Rennen", nextRace.Id);
                            return;
                        }

                        var done = false;
                        switch (boat.Status)
                        {
                            case UserControls.BoatStatus.BoatNok:
                                // Boot NOK => Anzeige NOK
                                status = ConfirmWindow.Status.Nok;
                                done = true;
                                break;

                            case UserControls.BoatStatus.BoatOk:
                                // Boot OK, Anzeige OK aber weiter untersuchen
                                status = ConfirmWindow.Status.Ok;
                                break;

                            case UserControls.BoatStatus.None:
                                // unbekannt => unbekannt anzeigen (wie kann das sein?)
                                status = ConfirmWindow.Status.Unknown;
                                done = true;
                                break;

                            case UserControls.BoatStatus.WaitingForTimeWindow:
                                // es ist noch zu früh zum Wiegen => warum ist der überhaupt gekommen?
                                status = ConfirmWindow.Status.Unknown;
                                break;

                            case UserControls.BoatStatus.WaitingInsideTimeWindow:
                                // warten auf (weitere/andere) Ruderer, Anzeige OK aber weiter untersuchen
                                status = ConfirmWindow.Status.Ok;
                                break;

                            default:
                                // nicht behandelter Fall => unklar
                                status = ConfirmWindow.Status.Unknown;
                                break;
                        }

                        if (!done)
                        {
                            // die restlichen Rennen des Ruderers weiter untersuchen
                            // das erste Rennen war OK, jetzt können die verbleibenden Rennen das Eegebnis noch verschlechtern
                            var remainingRaces = races.Skip(1);
                            foreach(var remainingRace in remainingRaces)
                            {
                                var remainingBoat = remainingRace.Boats.SingleOrDefault(b => (b.Rowers?.Any(r => r.Id == _id) == true));
                                if (remainingBoat == null)
                                {
                                    Tools.LogError("Boot mit diesem Ruderer in einem Folgerennen nicht gefunden oder mehrere Boote gefunden: RID", _id, "Rennen", remainingRace.Id);
                                }

                                switch (remainingBoat.Status)
                                {
                                    case UserControls.BoatStatus.BoatNok:
                                        // Boot NOK => Anzeige NOK
                                        status = ConfirmWindow.Status.Nok;
                                        break;

                                    case UserControls.BoatStatus.BoatOk:
                                        // nichts weiter tun
                                        break;

                                    case UserControls.BoatStatus.None:
                                        // unbekannt => unbekannt anzeigen (wie kann das sein?)
                                        status = ConfirmWindow.Status.Unknown;
                                        break;

                                    case UserControls.BoatStatus.WaitingForTimeWindow:
                                        // es gibt offenbar ein späteres Rennenen, für das es noch zu früh ist
                                        status = ConfirmWindow.Status.NearlyOk;
                                        break;

                                    case UserControls.BoatStatus.WaitingInsideTimeWindow:
                                        // es gibt offenbar ein späteres Rennenen (eventuell erst am nächsten Tag), für das es noch zu früh ist
                                        // nichts weiter tun
                                        break;

                                    default:
                                        // nicht behandelter Fall => unklar
                                        status = ConfirmWindow.Status.Unknown;
                                        break;
                                }
                            }
                        }
                    }

                    // Fenster anzeigen
                    var confirmWindow = new ConfirmWindow(status);
                    confirmWindow.ShowDialog();

                    #endregion
                }
                catch (Exception ex)
                {
                    // Fehler beim Konvertieren
                    Tools.ShowError("Ungültige Eingabe: " + ex.Message);
                }
                
                e.Handled = true;
            }
        }

        /// <summary>
        /// Formats the string.
        /// </summary>
        /// <param name="stringIn">The string in.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private string FormatString(string stringIn, int length)
        {
            if (stringIn.Length > length)
            {
                stringIn = stringIn.Substring(0, length);
            }

            while (stringIn.Length < length)
            {
                stringIn += " ";
            }

            return stringIn;
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
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            txtNewWeighting.Focus();
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
    }
}
