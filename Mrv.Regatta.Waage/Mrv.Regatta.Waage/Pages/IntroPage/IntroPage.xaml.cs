using System.Linq;
using System.Windows.Controls;

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
            _vm.RowersCount = GlobalData.Instance.RowersData?.Count() ?? 0;
            _vm.BoatsCount = GlobalData.Instance.RacesData?.Sum(r => r.BoatsData.Count) ?? 0;
            _vm.RacesCount = GlobalData.Instance.RacesData?.Count() ?? 0;
        }
        
    }
}
