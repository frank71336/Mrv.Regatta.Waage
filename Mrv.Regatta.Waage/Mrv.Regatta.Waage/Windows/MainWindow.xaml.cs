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

            // Einstellungen laden
            GlobalData.Instance.Settings = XmlBase.XmlBase.Load<Einstellungen>(new XmlBase.XmlFilePath(Properties.Settings.Default.SettingsFilePath));

            // Rennen laden
            // TODO: ???
            // GlobalData.Instance.RacesConfiguration = XmlBase.XmlBase.Load<Rennen>(new XmlBase.XmlFilePath(GlobalData.Instance.Settings.Pfade.Rennen));

            // Alles aus DB laden
            LoadDataFromDb();

            // Messungen laden
            Tools.ReadWeightings();

            /*

            // DB-Vereine laden
            LoadDbClubs();

            // Messungen laden
            Tools.ReadWeightings();

            // DB-Ruderer laden
            LoadDbRowers();

            // DB-Rennen laden
            LoadDbRaces();

            // DB-Boote laden
            LoadDbBoats();

            // DB-Boots-Abmeldungen laden
            LoadCancellations();

            */

            // ------------------------------ View-Model

            _vm = new MainViewModel();
            _vm.RacesReducedView = true;
            _vm.Day = GlobalData.Instance.Settings.ZeitstempelHeute.ToLongDateString();
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
        /// Loads the data from database.
        /// </summary>
        private void LoadDataFromDb()
        {
            var eventId = 3; // TODO: Muss irgendwie einstellbar sein



            var dbData = GlobalData.Instance;

            using (var db = new AquariusDataContext())
            {
                // Veranstaltungen laden
                var dbEvents = db.Events;
                dbData.EventsData = dbEvents.Select(e => new EventData() { Id = e.Event_ID, Title = e.Event_Title } ).ToList();

                // Altersklassen einlesen
                var dbAgeClasses = db.AgeClasses.ToList();

                // Bootsklassen einlesen
                var dbBoatClasses = db.BoatClasses.ToList();

                // Ausschreibung einlesen
                var dbOffers = db.Offers.Where(o => o.Offer_Event_ID_FK == eventId).ToList();

                // Wettkämpfe einlesen
                var dbCompetions = db.Comps.Where(c => (c.Comp_Event_ID_FK == eventId) && c.Comp_DateTime != null).OrderBy(c => c.Comp_DateTime);

                // Meldungen einlesen
                var dbEventEntries = db.Entries.Where(e => e.Entry_Event_ID_FK == eventId).ToList();

                // Mannschaften einlesen
                var dbCrews = db.Crews.ToList();

                // Ruderer einlesen
                var dbAthlets = db.Athlets.ToList();

                // Vereine einlesen
                var dbclubs = db.Clubs.ToList();

                // Rennen tauchen mehrfach auf, falls es z. B. mehrere Leistungsgruppen gibt.
                // Da die Läufe aber ggf. noch nicht eingeteilt sind, lassen sich die Personen nicht
                // den einzelnen Rennen/Läufen zuweisen.
                // Derartig mehrfach vorkommende Rennen werden daher zu einem Rennen zusammengefasst:
                var competitionGroups = dbCompetions.GroupBy(c => c.Comp_Race_ID_FK);

                #region Wettkämpfe durchgehen

                dbData.RacesData = new List<RaceData>();
                foreach (var competionGroup in competitionGroups)
                {
                    var offer = dbOffers.Single(o => o.Offer_ID == competionGroup.Key);
                    var ageClass = dbAgeClasses.Single(ac => ac.AgeClass_ID == offer.Offer_AgeClass_ID_FK);
                    var boatClass = dbBoatClasses.Single(bc => bc.BoatClass_ID == offer.Offer_BoatClass_ID_FK);

                    var isChildrenRace = (ageClass.AgeClass_MaxAge <= 14);
                    bool isLightweightRace = (offer.Offer_IsLightweight == 1);
                    var isCoxedRace = (boatClass.BoatClass_Coxed == 1);

                    // Entscheiden, ob man das Rennen übernehmen muss
                    var raceOk = false;
                    if (isLightweightRace) raceOk = true;      // alle Leichtgewichtsrennen müssen zur Waage
                    if (isCoxedRace && !isChildrenRace) raceOk = true; // gesteuerte Rennen, die keine Kinderrennen sind, müssen zur Waage

                    if (raceOk)
                    {
                        // Rennzeit ist die Zeit des ersten Rennens dieser Gruppe
                        var raceTime = (DateTime)competionGroup.Select(x => x).OrderBy(c => c.Comp_DateTime).First().Comp_DateTime;

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
                            IsLightweightRace = isLightweightRace,
                            MaxSingleWeight = new Weight(ageClass.AgeClass_LW_UpperLimit, true),
                            MaxAverageWeight = new Weight(ageClass.AgeClass_LW_AvgLimit, true),
                            MinCoxWeight = new Weight(ageClass.AgeClass_LW_CoxLowerLimit, true),
                            MaxAdditionalCoxWeight = new Weight(ageClass.AgeClass_LW_CoxTolerance, true)
                        };

                        dbData.RacesData.Add(newRace);

                        // zum aktuellen Rennen die Boote hinzufügen
                        var eventEntries = dbEventEntries.Where(ee => ee.Entry_Race_ID_FK == offer.Offer_ID).ToList();
                        newRace.BoatsData = new List<BoatData>();

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
                            var crew = dbCrews.Where(c => c.Crew_Entry_ID_FK == eventEntry.Entry_ID).ToList();

                            // Crew durchgehen und ins Boot platzieren
                            foreach (var crewMember in crew)
                            {
                                var athlet = dbAthlets.Single(a => a.Athlet_ID == crewMember.Crew_Athlete_ID_FK);
                                var club = dbclubs.Single(c => c.Club_ID == athlet.Athlet_Club_ID_FK);

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
