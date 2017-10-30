using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Mrv.Regatta.Waage.Pages.LicensesPage
{
    /// <summary>
    /// Interaction logic for LicensesPage.xaml
    /// </summary>
    public partial class LicensesPage : Page
    {
        LicensesPageViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogPage"/> class.
        /// </summary>
        public LicensesPage()
        {
            InitializeComponent();

            // Alle Lizenzen einlesen und aneinanderhängen
            var files = Directory.EnumerateFiles("licenses", "*.license", System.IO.SearchOption.TopDirectoryOnly);

            var licenseText = "";
            foreach(var file in files)
            {
                licenseText += File.ReadAllText(file);
                if (file != files.Last())
                {
                    var ret = "\r\n";
                    var line = "---------------------------------------------------------------------------------------------";
                    licenseText += ret + ret + ret + line + ret + ret + ret;
                }
            }

            _vm = new LicensesPageViewModel();
            _vm.LicensesText = licenseText;
            this.DataContext = _vm;
        }

    }
}
