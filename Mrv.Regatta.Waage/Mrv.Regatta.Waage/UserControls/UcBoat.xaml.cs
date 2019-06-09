using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mrv.Regatta.Waage.UserControls
{
    /// <summary>
    /// Interaction logic for Boat.xaml
    /// </summary>
    public partial class UcBoat : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UcBoat"/> class.
        /// </summary>
        public UcBoat()
        {
            InitializeComponent();

            (this.Content as FrameworkElement).DataContext = this;

            // Festlegen, ob die Ruderer links oder rechts angezeigt werden
            if (GlobalData.Instance.RowersPosition == RowersPosition.Left)
            {
                DiableRowersRight();
            }
            else
            {
                DiableRowersLeft();
            }
        }

        /// <summary>
        /// The boat property
        /// </summary>
        public static readonly DependencyProperty BoatProperty = DependencyProperty.Register("Boat", typeof(Boat), typeof(UcBoat), null);

        /// <summary>
        /// Gets or sets the boat.
        /// </summary>
        /// <value>
        /// The boat.
        /// </value>
        public Boat Boat
        {
            get
            {
                return (Boat)GetValue(BoatProperty);
            }
            set
            {
                SetValue(BoatProperty, value);
            }
        }

        /// <summary>
        /// Diables the rowers left.
        /// </summary>
        private void DiableRowersLeft()
        {
            var parentRowersLeft = VisualTreeHelper.GetParent(rowersLeft);
            var grid = (Grid)parentRowersLeft;
            grid.Children.Remove(rowersLeft);
            rowRowersLeft.Height = new GridLength(0);
        }

        /// <summary>
        /// Diables the rowers right.
        /// </summary>
        private void DiableRowersRight()
        {
            var parentRowersRight = VisualTreeHelper.GetParent(rowersRight);
            var grid = (Grid)parentRowersRight;
            grid.Children.Remove(rowersRight);

            // Spalten breite rechts auf 0 setzen
            columnRight.Width = new GridLength(0);
        }

    }
}
