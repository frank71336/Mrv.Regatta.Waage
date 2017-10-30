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

namespace Mrv.Regatta.Waage.UserControls
{
    /// <summary>
    /// Interaction logic for UcRowers.xaml
    /// </summary>
    public partial class UcRowers : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UcRowers"/> class.
        /// </summary>
        public UcRowers()
        {
            InitializeComponent();

            (this.Content as FrameworkElement).DataContext = this;
        }

        /// <summary>
        /// The boat property
        /// </summary>
        public static readonly DependencyProperty RowersProperty = DependencyProperty.Register("Rowers", typeof(List<Rower>), typeof(UcRowers), null);

        /// <summary>
        /// Gets or sets the rowers.
        /// </summary>
        /// <value>
        /// The rowers.
        /// </value>
        public List<Rower> Rowers
        {
            get
            {
                return (List<Rower>)GetValue(RowersProperty);
            }
            set
            {
                SetValue(RowersProperty, value);
            }
        }

        private void Rower_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var borderControl = (Border)sender;
            var id = Extensions.GetId(borderControl);

            var RowerPage = new Pages.RowerPage.RowerPage(id);
            Data.Instance.MainContent.Content = RowerPage;
        }
    }
}
