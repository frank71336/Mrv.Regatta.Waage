using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Mrv.Regatta.Waage.Pages.IntroPage
{
    /// <summary>
    /// Interaction logic for IntroPage.xaml
    /// </summary>
    public partial class IntroPage : Page
    {
        IntroPageViewModel _vm;

        public IntroPage()
        {
            InitializeComponent();

            _vm = new IntroPageViewModel();

            this.DataContext = _vm;

            _vm.Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            _vm.RowersCount = Data.Instance.DbRowers.Count();
            _vm.BoatsCount = Data.Instance.DbBoats.Count();
            _vm.RacesCount = Data.Instance.DbRaces.Count();
            _vm.ClubsCount = Data.Instance.DbClubs.Count();
        }
        
    }
}
