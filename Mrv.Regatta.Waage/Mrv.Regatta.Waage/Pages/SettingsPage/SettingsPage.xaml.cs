using Mrv.Regatta.Waage.Db.DataModels;
using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XmlBase.ArrayExtensions;

namespace Mrv.Regatta.Waage.Pages.SettingsPage
{
    /// <summary>
    /// Interaction logic for RowersPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the cmdCreateRennenXml control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdCreateRennenXml_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wirklich Rennen erzeugen?", "", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                return;
            }

            var settings = Data.Instance.Settings;

            var regeln = XmlBase.XmlBase.Load<Regeln>(new XmlBase.XmlFilePath(settings.Pfade.Regeln));
            var rules = regeln.Regel.ToList();

            var rennen = new Rennen();
            rennen.Rennen1 = new RennenRennen[] { };

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

                // Rennen durchgehen
                // Kurzbezeichnung (!) mit regulärem Ausdrück prüfen
                foreach (var race in races)
                {
                    var ok = false;
                    foreach(var rule in rules)
                    {
                        // Positive Bedingungen prüfen
                        foreach(var condition in rule.If)
                        {
                            var regEx = new Regex(condition.RegEx);
                            if (regEx.IsMatch(race.RKBez))
                            {
                                // Regel trifft zu => Rennen nehmen
                                ok = true;
                                break;
                            }
                        }

                        // Negative Bedingungen prüfen
                        if (ok && (rule.IfNot != null))
                        {
                            foreach (var condition in rule.IfNot)
                            {
                                var regEx = new Regex(condition.RegEx);
                                if (regEx.IsMatch(race.RKBez))
                                {
                                    // Regel trifft zu => Rennen doch nicht nehmen
                                    ok = false;
                                    break;
                                }
                            }
                        }

                        if (ok)
                        {
                            // Regel trifft zu
                            // => Rennen übernehmen

                            var newRace = new RennenRennen();
                            newRace.RennInfo = rule.RennInfo;
                            newRace.RennNr = race.RNr;
                            newRace.Aktiv = true;
                            rennen.Rennen1 = ArrayExtensions.AddArrayElement(rennen.Rennen1, newRace);

                            // restliche Regeln sind egal
                            break;
                        }
                    }
                }
            }

            rennen.Save(settings.Pfade.Rennen);
            Data.Instance.Races = rennen;

            MessageBox.Show("OK!");
        }

        /// <summary>
        /// Handles the Click event of the cmdSelfTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdSelfTest_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wirklich Selbsttest durchführen? Dabei werden neue Messungen erzeugt!", "", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                return;
            }

            var selfTestTask = new System.Threading.Tasks.Task(ExecuteSelfTest);
            selfTestTask.Start();
        }

        /// <summary>
        /// Executes the self test.
        /// </summary>
        private void ExecuteSelfTest()
        {
            // Ruderer anzeigen
            Tools.InvokeIfRequired(this, () =>
            {
                var rowersPage = new RowersPage.RowersPage();
                Data.Instance.MainContent.Content = rowersPage;
            });
            System.Threading.Thread.Sleep(1000);

            // Rennen anzeigen
            Tools.InvokeIfRequired(this, () =>
            {
                var racesPage = new RacesPage.RacesPage();
                Data.Instance.MainContent.Content = racesPage;
            });
            System.Threading.Thread.Sleep(7000);

            // 1. Durchgang durch alle Ruderer
            var rowers = Data.Instance.DbRowers;
            foreach(var rower in rowers)
            {
                // Einzelnen Ruderer anzeigen
                Tools.InvokeIfRequired(this, () =>
                {
                    var rowerPage = new RowerPage.RowerPage((int)rower.RID);
                    Data.Instance.MainContent.Content = rowerPage;
                });
                System.Threading.Thread.Sleep(750);
            }

            // Für alle Ruderer eine Messung für 10:00 Uhr ("heutiger" Tag) erzeugen
            SetRowersWeights(rowers, 65.5f, 10);

            // 2. Durchgang durch alle Ruderer
            foreach (var rower in rowers)
            {
                // Einzelnen Ruderer anzeigen
                Tools.InvokeIfRequired(this, () =>
                {
                    var rowerPage = new RowerPage.RowerPage((int)rower.RID);
                    Data.Instance.MainContent.Content = rowerPage;
                });
                System.Threading.Thread.Sleep(750);
            }

            // Für alle Ruderer eine zweite Messung für 14:00 Uhr ("heutiger" Tag) erzugen
            SetRowersWeights(rowers, 55.5f, 14);

            // 3. Durchgang durch alle Ruderer
            foreach (var rower in rowers)
            {
                // Einzelnen Ruderer anzeigen
                Tools.InvokeIfRequired(this, () =>
                {
                    var rowerPage = new RowerPage.RowerPage((int)rower.RID);
                    Data.Instance.MainContent.Content = rowerPage;
                });
                System.Threading.Thread.Sleep(500);
            }

            MessageBox.Show("Selbsttest abgeschlossen!");
        }

        /// <summary>
        /// Sets the rowers weights.
        /// </summary>
        /// <param name="rowers">The rowers.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="hour">The hour.</param>
        private void SetRowersWeights(List<TRuderer> rowers, float weight, int hour)
        {
            var day = Data.Instance.Settings.ZeitstempelHeute;
            var dt = new DateTime(day.Year, day.Month, day.Day, hour, 0, 0);

            var club = "Kein Verein bei Simulation";

            foreach (var rower in rowers)
            {
                // Speicherort
                var fileName = $"{dt.Year}-{dt.Month:00}-{dt.Day:00} {dt.Hour:00}-{dt.Minute:00}-{dt.Second:00} {Guid.NewGuid().ToString()}.xml";
                var path = Data.Instance.Settings.Pfade.Messungen;
                var filePath = System.IO.Path.Combine(path, fileName);

                // Daten der Messung
                var messung = new Messung();
                messung.Id = (int)rower.RID;
                messung.Name = $"{rower.RName}, {rower.RVorname} ({club})";
                messung.Zeitstempel = dt;
                messung.Gewicht = weight;

                // Messung speichern
                messung.Save(filePath);
            }

            // Messungen im Speicher aktualisieren
            Tools.ReadWeightings();
        }

    }
}
