using System;
using System.IO;
using System.Windows.Controls;

namespace Mrv.Regatta.Waage.Pages.LogPage
{
    /// <summary>
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : Page
    {
        LogPageViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogPage"/> class.
        /// </summary>
        public LogPage()
        {
            InitializeComponent();

            _vm = new LogPageViewModel();
            this.DataContext = _vm;

            try
            {
                _vm.LogText = File.ReadAllText(GlobalData.Instance.Settings.Pfade.Logdatei);
            }
            catch (Exception ex)
            {
                _vm.LogText = "Fehler bei Lesen der Log-Datei: " + ex.ToString();
            }
            
        }

    }
}
