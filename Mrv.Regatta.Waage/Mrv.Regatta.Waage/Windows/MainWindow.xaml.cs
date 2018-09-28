using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Mrv.Regatta.Waage.Db.DataModels;
using Mrv.Regatta.Waage.Xml;

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

            Data.Instance.MainContent = mainContent;

            // Einstellungen laden
            Data.Instance.Settings = XmlBase.XmlBase.Load<Einstellungen>(new XmlBase.XmlFilePath(Properties.Settings.Default.SettingsFilePath));

            // Rennen laden
            Data.Instance.RacesConfiguration = XmlBase.XmlBase.Load<Rennen>(new XmlBase.XmlFilePath(Data.Instance.Settings.Pfade.Rennen));

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

            // ------------------------------ View-Model

            _vm = new MainViewModel();
            _vm.RacesReducedView = true;
            _vm.Day = Data.Instance.Settings.ZeitstempelHeute.ToLongDateString();
            _vm.OverrideTime = false;

            this.DataContext = _vm;
            Data.Instance.MainViewModel = _vm;

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
        /// Loads the database boats.
        /// </summary>
        private void LoadDbBoats()
        {
            using (var db = new DatenDB())
            {
                var boats = db.TBootes.ToList();
                Data.Instance.DbBoats = boats;
            }
        }

        /// <summary>
        /// Loads the database boats.
        /// </summary>
        private void LoadCancellations()
        {
            using (var db = new DatenDB())
            {
                var cancellations = db.TAbmeldungs.ToList();
                Data.Instance.DbCancellations = cancellations;
            }
        }

        /// <summary>
        /// Loads the database races.
        /// </summary>
        private void LoadDbRaces()
        {
            using (var db = new DatenDB())
            {
                // alle Rennen, jedoch nur solche, die nach 1 Uhr morgens stattfinden
                // (Rennen, mit Rennzeit 00:00 Uhr sind nicht zustande gekommen)
                var races = db.TRennens.Where(x => ((DateTime)x.RZeit).TimeOfDay > new TimeSpan(1, 0, 0)).ToList();

                // Zeitstempel des Rennens korrigieren
                // Da steht nur die Uhrzeit. Das wird im DateTime-Format zum 01.01.0001 xx:xx.
                Tools.FixRaceTimestamp(races);

                // Rennen sortieren
                races = races.OrderBy(x => x.RZeit).ToList();

                Data.Instance.DbRaces = races;
            }
        }

        /// <summary>
        /// Loads the database clubs.
        /// </summary>
        private void LoadDbClubs()
        {
            using (var db = new DatenDB())
            {
                Data.Instance.DbClubs = db.TVereins.ToList();
            }
        }

        /// <summary>
        /// Loads the database rowers.
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        private void LoadDbRowers()
        {
            using (var db = new DatenDB())
            {
                var dbRowers = db.TRuderers.ToList();

                var rowers = new List<TRuderer>();

                switch (Data.Instance.Settings.PersonenListe)
                {
                    case EinstellungenPersonenListe.Alle:
                        throw new NotImplementedException("PersonenListe Typ 'alle' ist noch nicht implementiert!");

                    case EinstellungenPersonenListe.Regatta:
                        {
                            // alle Rennen, jedoch nur solche, die nach 1 Uhr morgens stattfinden
                            // (Rennen, mit Rennzeit 00:00 Uhr sind nicht zustande gekommen)
                            var dbRaces = db.TRennens.Where(x => ((DateTime)x.RZeit).TimeOfDay > new TimeSpan(1, 0, 0)).ToList();

                            // alle Boote
                            var dbBoats = db.TBootes.ToList();

                            // alle Rennen durchgehen
                            foreach (var race in dbRaces)
                            {
                                // alle Boote dieses Rennens
                                var boatsOfRace = dbBoats.Where(b => b.BRNr == race.Index).ToList();

                                // alle Boots des Rennens durchgehen
                                foreach (var boatOfRace in boatsOfRace)
                                {
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName1);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName2);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName3);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName4);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName5);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName6);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName7);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName8);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName9); // Steuermann
                                }
                            }
                        }

                        break;

                    case EinstellungenPersonenListe.Leichtgewichte:
                        {
                            // alle Rennen aus DB
                            var dbRaces = db.TRennens.ToList();

                            // alle Leichtgewicht-Rennen (aus XML-Datei)
                            var raceConfiguration = Data.Instance.RacesConfiguration.Rennen1;

                            // alle Boote
                            var dbBoats = db.TBootes.ToList();

                            // alle Rennen durchgehen
                            foreach (var race in raceConfiguration)
                            {
                                // den vollständigen Datensatz dazu zu diesem Rennen aus DB
                                var dbRace = dbRaces.SingleOrDefault(x => x.RNr == race.RennNr);
                                if (dbRace == null)
                                {
                                    Tools.LogError("DB-Rennen zu Rennen nicht gefunden oder mehrere gefunden! RNr", race.RennNr);
                                }

                                // alle Boote dieses Rennens
                                var boatsOfRace = dbBoats.Where(b => b.BRNr == dbRace.Index).ToList();

                                // alle Boots des Rennens durchgehen
                                foreach (var boatOfRace in boatsOfRace)
                                {
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName1);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName2);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName3);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName4);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName5);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName6);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName7);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName8);
                                    AddRower(dbRowers, ref rowers, boatOfRace.BName9); // Steuermann
                                }
                            }
                        }

                        break;

                    default:
                        throw new Exception($"Wert '{Data.Instance.Settings.PersonenListe.ToString()}' für Personenliste ist ungültig!");
                }

                Data.Instance.DbRowers = rowers.OrderBy(r => r.RName).ThenBy(r => r.RVorname).ToList();
            }
        }

        /// <summary>
        /// Ruderer zur Liste hinzufügen, wenn er nicht schon drin steht
        /// </summary>
        /// <param name="dbRowers">The database rowers.</param>
        /// <param name="rowers">The rowers.</param>
        /// <param name="rId">The rower id.</param>
        private void AddRower(List<TRuderer> dbRowers, ref List<TRuderer> rowers, int? rId)
        {
            // Wenn ein Platz im Boot nicht besetzt ist, dann steht "1" als Index drin (sofern nicht über das DRV-Meldeportal gemeldet wurde)
            // => in dem Fall ignorieren
            if ((rId != null) && (rId > 1))
            {
                if (!rowers.Any(r => r.RID == rId))
                {
                    var newRower = dbRowers.FirstOrDefault(r => r.RID == rId);
                    if (newRower != null)
                    {
                        rowers.Add(newRower);
                    }
                }
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
