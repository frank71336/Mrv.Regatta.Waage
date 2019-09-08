using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Mrv.Regatta.Waage.Xml;
using Mrv.Regatta.Waage.Db;
using Mrv.Regatta.Waage.DbData;

namespace Mrv.Regatta.Waage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        MainViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // ------------------------------ Globale Daten bereitstellen

            GlobalData.Instance.MainContent = mainContent;

            // Alles aus DB laden
            LoadDataFromDb();

            // Messungen laden
            Tools.ReadWeightings();

            CheckSettings();

            // ------------------------------ View-Model

            _vm = new MainViewModel();
            _vm.RacesReducedView = true;
            _vm.Day = Properties.Settings.Default.Today.ToLongDateString();
            _vm.OverrideTime = false;

            this.DataContext = _vm;
            GlobalData.Instance.MainViewModel = _vm;

            // Intro-Seite anzeigen
            var introPage = new Pages.IntroPage.IntroPage();
            mainContent.Content = introPage;

            // 1s-Timer für Uhr
            var t = new System.Timers.Timer();
            t.Elapsed += Timer_Elapsed;
            t.Interval = 1000;
            t.Start();
        }

        /// <summary>
        /// Checks the settings.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CheckSettings()
        {
            var settings = Properties.Settings.Default;

            // Hinweis:
            // Auf den Pfad zu den Messungen (settings.WeighingsPath) wurde bereits zugegriffen,
            // den muss man nicht nochmal testen

            // Hinweis: 
            // Backup -Verzeichnis wird nicht geprüft, Fehler wird erst beim Erstellen eines Backups angezeigt

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(settings.ErrorLogFile)))
            {
                MessageBox.Show($"Pfad für das Fehler-Log '{settings.ErrorLogFile}' existiert nicht. Einstellungen prüfen!");
                return;
            }

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(settings.WeighingsLogFile)))
            {
                MessageBox.Show($"Pfad für das Log der Messungen '{settings.WeighingsLogFile}' existiert nicht. Einstellungen prüfen!");
                return;
            }
        }

        /// <summary>
        /// Loads the data from database.
        /// </summary>
        private void LoadDataFromDb()
        {
            // Event-Id aus den Einstellungen holen
            var eventId = Properties.Settings.Default.EventId;

            using (var db = new AquariusDataContext(Properties.Settings.Default.ConnectionString))
            {
                // Test-Zugriff auf DB
                try
                {
                    var test = db.Events.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Datenbank-Zugriff! Verbindung zur Datenbank und Connection String in den Einstellungen prüfen.\r\n\r\nDetails:\r\n\r\n" + ex.ToString());
                    return;
                }

                var dbData = GlobalData.Instance;

                // Veranstaltungen laden
                var dbEvents = db.Events;
                dbData.EventsData = dbEvents.Select(e => new EventData() { Id = e.Event_ID, Title = e.Event_Title } ).ToList();
                if (dbData.EventsData.SingleOrDefault(ed => ed.Id == eventId) == null)
                {
                    MessageBox.Show("Veranstaltung nicht gefunden! Veranstaltung in den Einstellungen auswählen und Programm neu starten!");
                    return;
                }

                // Altersklassen einlesen
                var dbAgeClasses = db.AgeClasses.ToDictionary(ac => ac.AgeClass_ID, ac => ac);

                // Bootsklassen einlesen
                var dbBoatClasses = db.BoatClasses.ToDictionary(bc => bc.BoatClass_ID, bc => bc);

                // Ausschreibung einlesen
                var dbOffers = db.Offers.Where(o => o.Offer_Event_ID_FK == eventId).ToDictionary(o => o.Offer_ID, o => o);

                // Wettkämpfe einlesen
                var dbCompetions = db.Comps.Where(c => (c.Comp_Event_ID_FK == eventId) && c.Comp_DateTime != null).OrderBy(c => c.Comp_DateTime);

                // Läufe/Boote einlesen
                // (gruppiert nach Meldung)
                var dbEventEntries = db.Entries.Where(e => e.Entry_Event_ID_FK == eventId).GroupBy(ee => ee.Entry_Race_ID_FK).ToDictionary(ee => ee.Key, ee => ee.Select(x => x).ToList());

                // Mannschaften einlesen
                // (gruppiert nach dem Boot, in dem sie sitzt)
                var dbCrews = db.Crews.GroupBy(c => c.Crew_Entry_ID_FK).ToDictionary(c => c.Key, c => c.Select(x => x).ToList());

                // Ruderer einlesen
                var dbAthlets = db.Athlets.ToDictionary(a => a.Athlet_ID, a => a);

                // Vereine einlesen
                var dbClubs = db.Clubs.ToDictionary(c => c.Club_ID, c => c);

                // Rennen tauchen mehrfach auf, falls es z. B. mehrere Leistungsgruppen gibt.
                // Da die Läufe aber ggf. noch nicht eingeteilt sind, lassen sich die Personen nicht
                // den einzelnen Rennen/Läufen zuweisen.
                // Derartig mehrfach vorkommende Rennen werden daher zu einem Rennen zusammengefasst:
                var competitionGroups = dbCompetions.GroupBy(c => c.Comp_Race_ID_FK).ToList();

                #region Wettkämpfe durchgehen

                dbData.RacesData = new List<RaceData>();
                foreach (var competionGroup in competitionGroups)
                {
                    dbOffers.TryGetValue((int)competionGroup.Key, out var offer);
                    dbAgeClasses.TryGetValue(offer.Offer_AgeClass_ID_FK, out var ageClass);
                    dbBoatClasses.TryGetValue(offer.Offer_BoatClass_ID_FK, out var boatClass);

                    var isChildrenRace = (ageClass.AgeClass_MaxAge <= 14);
                    bool isLightweightRace = (offer.Offer_IsLightweight == 1);
                    var isCoxedRace = (boatClass.BoatClass_Coxed == 1);

                    // Entscheiden, ob man das Rennen übernehmen muss
                    var raceOk = false;
                    if (isLightweightRace) raceOk = true; // alle Leichtgewichtsrennen müssen zur Waage
                    if (isCoxedRace && !isChildrenRace) raceOk = true; // gesteuerte Rennen, die keine Kinderrennen sind, müssen zur Waage

                    if (raceOk)
                    {
                        // Rennzeit ist die Zeit des ersten Rennens dieser Gruppe
                        var raceTime = (DateTime)competionGroup.Select(x => x).OrderBy(c => c.Comp_DateTime).First().Comp_DateTime;

                        #region Gewichte bestimmen

                        var maxSingleWeight = new Weight(ageClass.AgeClass_LW_UpperLimit, true);
                        var maxAverageWeight = new Weight(ageClass.AgeClass_LW_AvgLimit, true);
                        var minCoxWeight = new Weight(ageClass.AgeClass_LW_CoxLowerLimit, true);
                        var maxAdditionalCoxWeight = new Weight(ageClass.AgeClass_LW_CoxTolerance, true);

                        // wenn es kein gesteuertes Rennen ist, dann interessiert das Gewicht des Steuermanns nicht
                        if (!isCoxedRace)
                        {
                            minCoxWeight.Value = null;
                        }

                        // bei Kinderrennen interessiert das Gewicht des Steuermanns ebenfalls nicht
                        if (isChildrenRace)
                        {
                            minCoxWeight.Value = null;
                        }

                        // bei Einer-Rennen kein Mannschaftsdurchschnittsgewicht
                        if (boatClass.BoatClass_NumRowers == 1)
                        {
                            maxAverageWeight.Value = null;
                        }

                        // Wenn es kein Leichgewichtsrennen ist, dann interessieren auch die Gewichte der Ruderer nicht
                        if (!isLightweightRace)
                        {
                            maxSingleWeight.Value = null;
                            maxAverageWeight.Value = null;
                        }

                        #endregion

                        // Rennen definieren und hinzufügen
                        var newRace = new RaceData()
                        {
                            Id = offer.Offer_ID,
                            RaceNumber = offer.Offer_RaceNumber,
                            ShortTitle = offer.Offer_ShortLabel,
                            LongTitle = offer.Offer_LongLabel,
                            DateTime = raceTime,
                            IsChildrenRace = isChildrenRace,
                            IsCoxedRace = isCoxedRace,
                            NumberOfRowers = boatClass.BoatClass_NumRowers,
                            IsLightweightRace = isLightweightRace,
                            MaxSingleWeight = maxSingleWeight,
                            MaxAverageWeight = maxAverageWeight,
                            MinCoxWeight = minCoxWeight,
                            MaxAdditionalCoxWeight = maxAdditionalCoxWeight
                        };

                        dbData.RacesData.Add(newRace);

                        // zum aktuellen Rennen die Boote hinzufügen
                        newRace.BoatsData = new List<BoatData>();
                        dbEventEntries.TryGetValue(offer.Offer_ID, out var eventEntries);

                        if (eventEntries != null)
                        {
                            foreach (var eventEntry in eventEntries)
                            {
                                var newBoat = new BoatData()
                                {
                                    TitleShort = eventEntry.Entry_ShortLabel,
                                    TitleLong = eventEntry.Entry_LongLabel,
                                    BibNumber = (byte)eventEntry.Entry_Bib,
                                    Canceled = (eventEntry.Entry_CancelValue > 0)
                                };

                                newRace.BoatsData.Add(newBoat);

                                // Zum aktuellen Boot die Mannschaft hinzufügen
                                newBoat.Rowers = new List<RowerData>();
                                dbCrews.TryGetValue(eventEntry.Entry_ID, out var crew);

                                if (crew != null)
                                {
                                    // Crew durchgehen und ins Boot platzieren
                                    foreach (var crewMember in crew)
                                    {
                                        dbAthlets.TryGetValue(crewMember.Crew_Athlete_ID_FK, out var athlet);
                                        dbClubs.TryGetValue((int)athlet.Athlet_Club_ID_FK, out var club);

                                        if ((athlet != null) && (club != null))
                                        {
                                            var newRower = new RowerData()
                                            {
                                                Id = athlet.Athlet_ID,
                                                ClubTitleLong = club.Club_Name,
                                                ClubTitleShort = club.Club_Abbr,
                                                LastName = athlet.Athlet_LastName,
                                                FirstName = athlet.Athlet_FirstName,
                                                DateOfBirth = athlet.Athlet_DOB,
                                                Gender = (athlet.Athlet_Gender == 'M') ? Gender.Male : Gender.Female
                                            };

                                            if (crewMember.Crew_IsCox)
                                            {
                                                // es handelt sich um den Steuermann
                                                newBoat.Cox = newRower;
                                            }
                                            else
                                            {
                                                // normaler Ruderer
                                                newBoat.Rowers.Add(newRower);
                                            }
                                        }
                                        else
                                        {
                                            // Fehler: Ruderer oder Verein nicht gefunden
                                            Tools.LogErrorNoMessageBox("Warnung: Ruderer oder kein Verein zu Crew nicht gefunden. Rennen ", offer.Offer_RaceNumber, offer.Offer_ShortLabel);
                                        }
                                    }
                                }
                                else
                                {
                                    // Fehler: keine Mannschaft für das Boot gefunden
                                    Tools.LogErrorNoMessageBox("Warnung: Mannschaft zu Rennen nicht gefunden. Rennen ", offer.Offer_RaceNumber, offer.Offer_ShortLabel);
                                }
                            }
                        }

                        // Boote sortieren nach Startnummer
                        newRace.BoatsData = newRace.BoatsData.OrderBy(b => b.BibNumber).ToList();
                    }
                }

                #endregion

                #region Ruderer anhand der ermittelten Rennen bestimmen

                var newRowers = new List<RowerData>();
                foreach(var raceData in dbData.RacesData)
                {
                    foreach(var boatData in raceData.BoatsData)
                    {
                        // Ruderer hinzufügen
                        foreach (var rowerData in boatData.Rowers)
                        {
                            if (!newRowers.Any(r => r.Id == rowerData.Id))
                            {
                                newRowers.Add(rowerData);
                            }
                        }

                        // Steuermann hinzufügen
                        if (boatData.Cox != null)
                        {
                            if (!newRowers.Any(r => r.Id == boatData.Cox.Id))
                            {
                                newRowers.Add(boatData.Cox);
                            }
                        }
                    }
                }

                dbData.RowersData = newRowers.OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ToList();

                #endregion

                // Rennen sortieren nach Startzeit
                dbData.RacesData = dbData.RacesData.OrderBy(r => r.DateTime).ToList();
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the Timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Uhrzeit aktualisieren
            _vm.CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            if (!_vm.OverrideTime)
            {
                _vm.ManualTime = _vm.CurrentTime;
            }
        }

        /// <summary>
        /// Handles the Click event of the cmdPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdPersons_Click(object sender, RoutedEventArgs e)
        {
            var RowersPage = new Pages.RowersPage.RowersPage();
            mainContent.Content = RowersPage;
        }

        /// <summary>
        /// Handles the Click event of the cmdRaces control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdRaces_Click(object sender, RoutedEventArgs e)
        {
            var racesPage = new Pages.RacesPage.RacesPage();
            mainContent.Content = racesPage;
        }

        /// <summary>
        /// Handles the Click event of the cmdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingPage = new Pages.SettingsPage.SettingsPage();
            mainContent.Content = settingPage;
        }

        /// <summary>
        /// Inverse Boolean Value Converter for binding.
        /// </summary>
        /// <seealso cref="System.Windows.Data.IValueConverter" />
        [ValueConversion(typeof(bool), typeof(bool))]
        public class InverseBooleanConverter : IValueConverter
        {
            #region IValueConverter Members

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (targetType != typeof(bool))
                {
                    throw new InvalidOperationException("The target must be a boolean!");
                }

                return !(bool)value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        /// <summary>
        /// Lässt nur Uhrzeiten (mit Doppelpunkten) in der Textbox zu.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the event data.</param>
        private void TimeOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextTime(e.Text);
        }

        /// <summary>
        /// Determines whether string is a time.
        /// </summary>
        /// <param name="str">The string.</param>
        private static bool IsTextTime(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"[0-9:]");
            return reg.IsMatch(str);
        }

        /// <summary>
        /// Handles the Click event of the cmdLogfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdLogfile_Click(object sender, RoutedEventArgs e)
        {
            var logPage = new Pages.LogPage.LogPage();
            mainContent.Content = logPage;
        }

        /// <summary>
        /// Handles the Click event of the cmdClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wirklich beenden?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the MetroWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void MetroWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Taste gedrückt - globale Abfrage egal welche Seite gerade angezeigt wird
            switch (e.Key)
            {
                case System.Windows.Input.Key.F1:
                    // F1: Ruderer anzeigen
                    var RowersPage = new Pages.RowersPage.RowersPage();
                    mainContent.Content = RowersPage;
                    break;

                case System.Windows.Input.Key.F2:
                    // F2: Rennen anzeigen
                    var racesPage = new Pages.RacesPage.RacesPage();
                    mainContent.Content = racesPage;
                    break;

                case System.Windows.Input.Key.F3:
                    // F3: Log anzeigen
                    var logPage = new Pages.LogPage.LogPage();
                    mainContent.Content = logPage;
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the cmdCertificate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdCertificate_Click(object sender, RoutedEventArgs e)
        {
            var licensePage = new Pages.LicensesPage.LicensesPage();
            mainContent.Content = licensePage;
        }
    }
}
