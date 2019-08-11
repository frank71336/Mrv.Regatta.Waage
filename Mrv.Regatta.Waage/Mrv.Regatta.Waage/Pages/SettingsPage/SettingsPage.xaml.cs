using Mrv.Regatta.Waage.Db;
using Mrv.Regatta.Waage.DbData;
using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ViewModelBase.CollectionExtensions;

namespace Mrv.Regatta.Waage.Pages.SettingsPage
{
    /// <summary>
    /// Interaction logic for RowersPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private SettingsPageViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            var settings = Properties.Settings.Default;

            var selectedEvent = GlobalData.Instance.EventsData?.SingleOrDefault(ed => ed.Id == settings.EventId);

            _vm = new SettingsPageViewModel()
            {
                ConnectionString = settings.ConnectionString,
                Today = settings.Today,
                BackupPath = settings.BackupPath,
                ErrorLogFile = settings.ErrorLogFile,
                Events = GlobalData.Instance.EventsData.ToObservableCollection(),
                Event = selectedEvent,
                WeighingsLogFile = settings.WeighingsLogFile,
                WeighingsPath = settings.WeighingsPath
            };
            
            this.DataContext = _vm;
        }

        /// <summary>
        /// Handles the Click event of the cmdSaveSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdSaveSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;

            var eventData = (EventData)_vm.Event;
            var eventId = (eventData != null) ? eventData.Id : settings.EventId;

            // Einstellungen übernehmen
            
            settings.ConnectionString = _vm.ConnectionString;
            settings.Today = _vm.Today;
            settings.BackupPath = _vm.BackupPath;
            settings.ErrorLogFile = _vm.ErrorLogFile;
            settings.EventId = eventId;
            settings.WeighingsLogFile = _vm.WeighingsLogFile;
            settings.WeighingsPath = _vm.WeighingsPath;

            // Einstellungen speichern
            Properties.Settings.Default.Save();

            // Anzeige aktualisieren
            GlobalData.Instance.MainViewModel.Day = Properties.Settings.Default.Today.ToLongDateString();
        }

        /// <summary>
        /// Handles the Click event of the cmdTestDb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdTestDb_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var db = new AquariusDataContext(_vm.ConnectionString))
            {
                // Test-Zugriff auf DB
                try
                {
                    var test = db.Events.ToList();
                    MessageBox.Show("OK!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Datenbank-Zugriff: " + ex.ToString());
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the cmdRestart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdRestart_Click(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        private void Restart()
        {
            var exeName = AppDomain.CurrentDomain.FriendlyName;
            var startInfo = new ProcessStartInfo
            {
                FileName = exeName
            };
            Process.Start(startInfo);

            // sich selbst beenden
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the cmdSelfTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdSelfTest_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wirklich Selbsttest durchführen? Dabei werden neue zufällige Messungen erzeugt!", "", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
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
                GlobalData.Instance.MainContent.Content = rowersPage;
            });
            System.Threading.Thread.Sleep(1000);

            // Rennen anzeigen
            Tools.InvokeIfRequired(this, () =>
            {
                var racesPage = new RacesPage.RacesPage();
                GlobalData.Instance.MainContent.Content = racesPage;
            });
            System.Threading.Thread.Sleep(3000);

            // 1. Durchgang durch alle Ruderer
            var rowers = GlobalData.Instance.RowersData;
            foreach (var rower in rowers)
            {
                // Einzelnen Ruderer anzeigen
                Tools.InvokeIfRequired(this, () =>
                {
                    var rowerPage = new RowerPage.RowerPage((int)rower.Id);
                    GlobalData.Instance.MainContent.Content = rowerPage;
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
                    var rowerPage = new RowerPage.RowerPage(rower.Id);
                    GlobalData.Instance.MainContent.Content = rowerPage;
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
                    var rowerPage = new RowerPage.RowerPage(rower.Id);
                    GlobalData.Instance.MainContent.Content = rowerPage;
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
        private void SetRowersWeights(List<RowerData> rowers, float weight, int hour)
        {
            var day = Properties.Settings.Default.Today;
            var dt = new DateTime(day.Year, day.Month, day.Day, hour, 0, 0);

            var club = "Kein Verein bei Simulation";

            foreach (var rower in rowers)
            {
                // Speicherort
                var fileName = $"{dt.Year}-{dt.Month:00}-{dt.Day:00} {dt.Hour:00}-{dt.Minute:00}-{dt.Second:00} {Guid.NewGuid().ToString()}.xml";
                var path = Properties.Settings.Default.WeighingsPath;
                var filePath = System.IO.Path.Combine(path, fileName);

                // Daten der Messung
                var messung = new Messung
                {
                    Id = rower.Id,
                    Name = $"{rower.LastName}, {rower.FirstName} ({club})",
                    Zeitstempel = dt,
                    Gewicht = weight
                };

                // Messung speichern
                messung.Save(filePath);
            }

            // Messungen im Speicher aktualisieren
            Tools.ReadWeightings();
        }

        /*
        
        /// <summary>
        /// Handles the Click event of the cmdBackup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdBackup_Click(object sender, RoutedEventArgs e)
        {
            var paths = GlobalData.Instance.Settings.Pfade;
            var backupRoot = paths.Backups;

            if (!Directory.Exists(backupRoot))
            {
                // Backup-Verzeichnis erstellen falls es nicht existiert
                Directory.CreateDirectory(backupRoot);
            }

            var backupFolder = $"backup-{DateTime.Now.ToString("yy-MM-dd HH.mm.ss")}";
            var backupDir = System.IO.Path.Combine(backupRoot, backupFolder);

            // Backup-Verzeichnis erzeugen
            Directory.CreateDirectory(backupDir);

            // Dateien kopieren
            CopyToDirectory(paths.Logdatei, backupDir);
            CopyToDirectory(paths.Regeln, backupDir);
            CopyToDirectory(paths.Rennen, backupDir);
            DirectoryCopy(paths.Messungen, System.IO.Path.Combine(backupDir, "Messungen"), false);

            // Zip erzeugen
            var zipFile = backupDir + ".zip";
            System.IO.Compression.ZipFile.CreateFromDirectory(backupDir, zipFile);

            // Zip-Datei auf USB-Sticks kopieren

            // alle bereiten Wechselmedien bestimmen
            var removableDrives = DriveInfo.GetDrives().ToList().Where(d => (d.DriveType == DriveType.Removable) && (d.IsReady));

            if (removableDrives?.Any() != true)
            {
                // Es gibt keine bereiten Wechselmedien
                MessageBox.Show("Backup lokal erstellt aber keine Backup-Medien gefunden!");
                return;
            }

            // bereiten Wechselmedien durchgehen und Backup kopieren
            foreach (var drive in removableDrives)
            {
                var driveBackupFolder = System.IO.Path.Combine(drive.Name, "backups"); // TODO: Ist 'drive.Name' das richtige Property?

                if (!Directory.Exists(driveBackupFolder))
                {
                    // Backup-Verzeichnis erstellen falls es nicht existiert
                    Directory.CreateDirectory(driveBackupFolder);
                }

                // Zip kopieren
                var zipBackupFile = System.IO.Path.Combine(backupDir, zipFile);
                CopyToDirectory(zipBackupFile, driveBackupFolder);
            }

            MessageBox.Show("Backup erstellt.");
        }

        /// <summary>
        /// Copies a file to a directory.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationDir">The destination dir.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        private static void CopyToDirectory(string sourceFile, string destinationDir, bool overwrite = false)
        {
            var destinationFile = System.IO.Path.Combine(destinationDir, System.IO.Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destinationFile, overwrite);
        }

        /// <summary>
        /// Directories the copy.
        /// </summary>
        /// <param name="sourceDirName">Name of the source dir.</param>
        /// <param name="destDirName">Name of the dest dir.</param>
        /// <param name="copySubDirs">if set to <c>true</c> [copy sub dirs].</param>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found: "
        ///                     + sourceDirName</exception>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        */
    }
}
