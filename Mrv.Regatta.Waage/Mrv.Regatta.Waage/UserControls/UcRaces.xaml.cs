using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Races.xaml
    /// </summary>
    public partial class UcRaces : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UcRaces"/> class.
        /// </summary>
        public UcRaces()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The races property
        /// </summary>
        public static readonly DependencyProperty RacesProperty = DependencyProperty.Register("Races", typeof(ObservableCollection<Race>), typeof(UcRaces), null);

        /// <summary>
        /// Gets or sets the races.
        /// </summary>
        /// <value>
        /// The races.
        /// </value>
        public ObservableCollection<Race> Races
        {
            get
            {
                return (ObservableCollection<Race>)GetValue(RacesProperty);
            }
            set
            {
                SetValue(RacesProperty, value);
            }
        }

        /// <summary>
        /// Gets the ItemsControl.
        /// </summary>
        /// <returns></returns>
        public ItemsControl GetItemsControl()
        {
            return itemsControl;
        }

        /// <summary>
        /// Gets the ScrollViewer.
        /// </summary>
        /// <returns></returns>
        public ScrollViewer GetScrollViewer()
        {
            return scrollViewer;
        }

    }
}
