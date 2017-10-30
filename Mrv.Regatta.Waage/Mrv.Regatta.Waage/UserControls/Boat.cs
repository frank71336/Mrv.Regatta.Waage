using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mrv.Regatta.Waage.UserControls
{

    public class Boat : ViewModelBase.ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Boat"/> class.
        /// </summary>
        public Boat()
        {
            Comment = "";
            Visibility = Visibility.Visible;
        }

        public int Id { get; set; }
        public string StartNumber { get; set; }
        public string Club { get; set; }
        public string AverageWeight { get; set; }

        public string Comment { get; set; }

        public BoatStatus Status { get; set; }

        /// <summary>
        /// Gibt an ob das Boot abgemeldet wurde
        /// </summary>
        /// <value>
        ///   <c>true</c> if canceled; otherwise, <c>false</c>.
        /// </value>
        public bool Canceled { get; set; }

        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        /// <value>
        /// The visibility.
        /// </value>
        public Visibility Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                OnPropertyChanged("Visibility");
            }
        }
        private Visibility _visibility;

        public List<Rower> Rowers { get; set; }
    }
}
