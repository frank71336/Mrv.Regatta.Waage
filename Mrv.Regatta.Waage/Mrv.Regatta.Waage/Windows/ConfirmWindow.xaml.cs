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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mrv.Regatta.Waage.Windows
{
    /// <summary>
    /// Interaction logic for OkWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OkWindow" /> class.
        /// </summary>
        /// <param name="status">The status.</param>
        public ConfirmWindow(Status status)
        {
            InitializeComponent();

            imgOk.Visibility = (status == Status.Ok) ? Visibility.Visible : Visibility.Hidden;
            imgOk2.Visibility = (status == Status.NearlyOk) ? Visibility.Visible : Visibility.Hidden;
            imgError.Visibility = (status == Status.Nok) ? Visibility.Visible : Visibility.Hidden;
            imgUnknown.Visibility = (status == Status.Unknown) ? Visibility.Visible : Visibility.Hidden;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += TimerTick;
            timer.Start();
        }

        /// <summary>
        /// Handles the Tick event of the timer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            Close();
        }

        public enum Status
        {
            Ok,
            Nok,
            NearlyOk,
            Unknown
        }

    }
}
