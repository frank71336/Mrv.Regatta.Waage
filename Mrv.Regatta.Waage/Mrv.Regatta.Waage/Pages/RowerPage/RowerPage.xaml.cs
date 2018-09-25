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

namespace Mrv.Regatta.Waage.Pages.RowerPage
{
    /// <summary>
    /// Interaction logic for RowerPage.xaml
    /// </summary>
    public partial class RowerPage : Page
    {
        private TimeSpan _currentTime;

        private TimeSpan _delayTime;

        private RowerPageViewModel _vm;

        private int _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowerPage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public RowerPage(int id)
        {
            InitializeComponent();

            Data.Instance.RowersPosition = RowersPosition.Left;

            _id = id;

            _vm = new RowerPageViewModel();
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

            #region Infos zum Ruderer

            var dbRower = dbRowers.SingleOrDefault(x => x.RID == _id);
            if (dbRower == null)
            {
                Tools.LogError("DB-Ruderer nicht gefunden oder mehrere gefunden: RID", _id);
            }

            var dbClub = dbClubs.SingleOrDefault(x => x.VIDVerein == dbRower.RVerein);
            if (dbClub == null)
            {
                Tools.LogError("DB-Verein zum Ruderer nicht gefunden oder mehrere gefunden! VIDVerein", dbRower.RVerein);
            }

            _vm.Name = $"{dbRower.RName}, {dbRower.RVorname}";
            _vm.Sex = dbRower.Geschlecht.ToString().Equals("w", StringComparison.OrdinalIgnoreCase) ? Sex.Female : Sex.Male;
            _vm.BirthYear = dbRower.RJg.ToString();
            _vm.Club = dbClub.VVereinsName;

            #endregion

            #region bisherige Gewichtsmessungen

            // Deutsche Bezeichnung für die Wochentage
            var culture = new System.Globalization.CultureInfo("de-DE");

            // Alle bisherigen Gewichtsmessungen
            _vm.Weightings = Data.Instance.Weightings.Where(x => x.Id == _id).OrderByDescending(x => x.Zeitstempel).Select(x => new RowerPageViewModel.Weighting()
            {
                Mass = x.Gewicht.ToString(),
                Date = culture.DateTimeFormat.GetDayName(x.Zeitstempel.DayOfWeek),
                Time = $"{x.Zeitstempel:HH:mm:ss}"

            }).ToObservableCollection();

            #endregion

            #region Rennen

            // alle Boote, in denen unser Ruderer sitzt
            var boatsThisRower = dbBoats.Where(b =>
            (
                (b.BName1 == _id) ||
                (b.BName2 == _id) ||
                (b.BName3 == _id) ||
                (b.BName4 == _id) ||
                (b.BName5 == _id) ||
                (b.BName6 == _id) ||
                (b.BName7 == _id) ||
                (b.BName8 == _id) ||
                (b.BName9 == _id)
            ));

            // nur Boote nehmen, die eine sinnvolle Startnummer haben
            boatsThisRower = boatsThisRower.Where(b => (!string.IsNullOrWhiteSpace(b.BSNr)) && (b.BSNr.Trim() != "0"));

            // die zugehörigen Rennen
            var racesThisRower = boatsThisRower.GroupBy(b => b.BRNr);

            // aktueller Zeitstempel
            var day = Data.Instance.Settings.ZeitstempelHeute;
            var now = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            // Alle Rennen mit unserem Ruderer durchgehen
            _vm.Races = new System.Collections.ObjectModel.ObservableCollection<UserControls.Race>();
            foreach (var raceThisRower in racesThisRower)
            {
                var dbRace = dbRaces.SingleOrDefault(r => r.Index == raceThisRower.Key);

                // gibt es überhaupt ein Rennen zu diesem Boot?
                if (dbRace == null)
                {
                    // Rennen existiert nicht !?!
                    // TODO: Gibt es ein Beispiel, wann das vorkommt?
                    continue;
                }

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
                        RaceDT = (DateTime)dbRace.RZeit + _delayTime,
                        ShortName = dbRace.RKBez,
                        LongName = dbRace.RLBez,
                        Day = dbRace.RTag,
                        Time = ((DateTime)dbRace.RZeit + _delayTime).ToString("HH:mm"), // korrigierte Zeit mit Verspätung
                        ScheduledTime = ((DateTime)dbRace.RZeit).ToString("HH:mm"), // ursprüngliche Zeit
                        ScheduledTimeVisibility = Data.Instance.MainViewModel.SetDelayTime ? Visibility.Visible : Visibility.Collapsed,
                        MaxWeight1 = maxWeight1,
                        MaxWeightAverage = maxWeightAverage,
                        MinWeightCox = minWeightCox,
                        DbRace = dbRace,
                        Boats = new System.Collections.ObjectModel.ObservableCollection<UserControls.Boat>()
                    };

                    // Boote nur mit diesem Ruderer drin zum Rennen hinzufügen
                    var boats = boatsThisRower.Where(b => b.BRNr == dbRace.Index).OrderBy(b => b.BSNr);

                    // Alle Boote des Rennens
                    foreach (var boat in boats)
                    {
                        // zuerst den Verein zum Boot ermitteln
                        var club = dbClubs.SingleOrDefault(c => c.VIDVerein == boat.BVID);
                        if (club == null)
                        {
                            Tools.LogError("DB-Verein nicht gefunden oder mehrere gefunden: Rennen", dbRace.RNr, "Boot", boat.BSNr, "Vereins-Id.", boat.BVID);
                        }

                        // Neues Boot
                        var newBoat = new UserControls.Boat()
                        {
                            Id = boat.BID,
                            StartNumber = boat.BSNr,
                            Club = club?.VVereinsnamenKurz,
                            Status = UserControls.BoatStatus.None
                        };

                        // Die Ruderer des Bootes
                        var rowers = new List<UserControls.Rower>();

                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName1, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName2, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName3, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName4, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName5, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName6, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName7, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName8, false, _delayTime);
                        boatBuilder.AddRower(race, dbRace, ref rowers, boat.BName9, true, _delayTime);

                        // Ruderer zum Boot hinzufügen
                        newBoat.Rowers = rowers;

                        // Status-Symbol des Boots entsprechend des Status der Ruderer im Boot setzen
                        boatBuilder.SetBoatStatus(newRace, newBoat);

                        // Boote zum Rennen hinzufügen
                        newRace.Boats.Add(newBoat);
                    }

                    // Status-Symbol des Rennens entsprechend des Status der Boote im Rennen setzen
                    boatBuilder.SetRaceStatus(newRace);

                    newRace.UpdateRemainingMinutes(now);

                    // Rennen hinzufügen
                    _vm.Races.Add(newRace);
                }
            }

            _vm.Races = _vm.Races.OrderBy(r => r.RaceDT).ToObservableCollection();

            #endregion

            #region Kommentar

            // das nächste Rennen des Ruderers ermitteln

            // alle Rennen des Ruderers (nur die, die in der View auch angezeigt werden)
            var races1 = dbRaces.Where(dbR => (_vm.Races?.Any(r => r.RaceNumber == dbR.RNr) == true)).OrderBy(r => r.RZeit).ToList();

            // alle Rennen, die noch kommen
            var races2 = races1.Where(r => (r.RZeit + _delayTime)  > now);

            var nextRace = races2.FirstOrDefault();

            if (nextRace == null)
            {
                // es gibt keine weiteren Rennen mehr
                _vm.Comment = "Keine weiteren Rennen mehr.";
                _vm.CommentBrush = new SolidColorBrush(Colors.LightSkyBlue);
            }
            else
            {
                var timeDiff = (DateTime)(nextRace.RZeit + _delayTime) - now;

                if (timeDiff.TotalHours > 2)
                {
                    // es ist noch länger als 2 Stunden bis zum nächsten Rennen

                    // jetzt kommt es noch drauf an, ob es ein Kinderrennen ist
                    var raceTest = races.Rennen1.SingleOrDefault(r => r.RennNr == nextRace.RNr);
                    if (raceTest == null)
                    {
                        // Rennen in den Einstellungen gar nicht gefunden, das ist ganz schlecht
                        _vm.CommentBrush = new SolidColorBrush(Colors.Gray);
                    }
                    else
                    {
                        if (raceTest.RennInfo.Kinderrennen)
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

                }
                else
                {
                    // es ist weniger als 2 Stunden bis zum nächsten Rennen
                    _vm.CommentBrush = new SolidColorBrush(Colors.LightGreen);
                }

                _vm.Comment = $"Nächstes Rennen ({nextRace.RNr}): {timeDiff.Hours} h {timeDiff.Minutes} min.";
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
                    var rower = Data.Instance.DbRowers.SingleOrDefault(x => x.RID == _id);
                    if (rower == null)
                    {
                        Tools.LogError("DB-Ruderer nicht gefunden oder mehrere gefunden: RID", _id);
                    }

                    var club = Data.Instance.DbClubs.SingleOrDefault(x => x.VIDVerein == rower.RVerein);
                    if (club == null)
                    {
                        Tools.LogError("DB-Verein zum Ruderer nicht gefunden oder mehrere gefunden! VIDVerein", rower.RVerein);
                    }

                    // Zeitstempel
                    var day = Data.Instance.Settings.ZeitstempelHeute;

                    // "RefreshCurrentTime" kann Exception werfen, die wird aber weiter unten schon abgefangen
                    RefreshCurrentTime(); // lokal gespeicherte Zeit aktualisieren
                    var dt = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

                    // Speicherort
                    var fileName = $"{dt.Year}-{dt.Month:00}-{dt.Day:00} {dt.Hour:00}-{dt.Minute:00}-{dt.Second:00} {Guid.NewGuid().ToString()}.xml";
                    var path = Data.Instance.Settings.Pfade.Messungen;
                    var filePath = Path.Combine(path, fileName);

                    // Daten der Messung
                    var messung = new Messung();
                    messung.Id = _id;
                    messung.Name = $"{rower.RName}, {rower.RVorname} ({club.VKurzzeichen})";
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
                    using (StreamWriter streamWriter = File.AppendText(Data.Instance.Settings.Pfade.Logdatei))
                    {
                        var logName = FormatString(_vm.Name, 30);
                        var logClub = FormatString(_vm.Club, 25);
                        streamWriter.WriteLine($"{logName} | {_vm.BirthYear} | {logClub} | {dt} | {value}");
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
            _currentTime = Data.Instance.GetCurrentTime();
            _delayTime = Data.Instance.GetCurrentDelay();
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
    }
}
