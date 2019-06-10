using Mrv.Regatta.Waage.Properties;
using System.Windows;

namespace Mrv.Regatta.Waage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Handles the Startup event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs"/> instance containing the event data.</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Upgrade der Programmeinstellungen:
            // https://stackoverflow.com/questions/534261/how-do-you-keep-user-config-settings-across-different-assembly-versions-in-net/534335#534335
            // In order to run the merge whenever you publish a new version of your application you can define a boolean flag in your settings file
            // that defaults to true.Name it UpgradeRequired or something similar. Then, at application start you check to see if the flag is set
            // and if it is, call the Upgrade method, set the flag to false and save your configuration.
            if (Settings.Default._UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default._UpgradeRequired = false;
                Settings.Default.Save();
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
