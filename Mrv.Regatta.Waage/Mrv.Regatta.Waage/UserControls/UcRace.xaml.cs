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
    /// Interaction logic for Race.xaml
    /// </summary>
    public partial class UcRace : UserControl
    {
        public UcRace()
        {
            InitializeComponent();

            (this.Content as FrameworkElement).DataContext = this;
        }

        /// <summary>
        /// The race property
        /// </summary>
        public static readonly DependencyProperty RaceProperty = DependencyProperty.Register("Race", typeof(Race), typeof(UcRace), null);

        /// <summary>
        /// Gets or sets the race.
        /// </summary>
        /// <value>
        /// The races.
        /// </value>
        public Race Race
        {
            get
            {
                return (Race)GetValue(RaceProperty);
            }
            set
            {
                SetValue(RaceProperty, value);
            }
        }
    }
}
