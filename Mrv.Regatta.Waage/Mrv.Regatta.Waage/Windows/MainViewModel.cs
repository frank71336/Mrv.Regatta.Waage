using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage
{
    public class MainViewModel : ViewModelBase.ViewModelBase
    {
        public string Day { get; set; }

        
        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        /// <value>
        /// The current time.
        /// </value>
        public string CurrentTime
        {
            get
            {
                return _currentTime;
            }
            set
            {
                _currentTime = value;
                OnPropertyChanged("CurrentTime");
            }
        }

        private string _currentTime;

        /// <summary>
        /// Gets or sets a value indicating whether [override time].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [override time]; otherwise, <c>false</c>.
        /// </value>
        public bool OverrideTime
        {
            get
            {
                return _overrideTime;
            }
            set
            {
                _overrideTime = value;
                OnPropertyChanged("OverrideTime");
            }
        }

        private bool _overrideTime;

        /// <summary>
        /// Gets or sets the manual time.
        /// </summary>
        /// <value>
        /// The manual time.
        /// </value>
        public string ManualTime
        {
            get
            {
                return _manualTime;
            }
            set
            {
                _manualTime = value;
                OnPropertyChanged("ManualTime");
            }
        }

        private string _manualTime;
    }
}
