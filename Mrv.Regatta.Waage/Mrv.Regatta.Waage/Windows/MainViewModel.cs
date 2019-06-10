namespace Mrv.Regatta.Waage
{
    public class MainViewModel : ViewModelBase.ViewModelBase
    {
        /// <summary>
        /// Gets or sets the current day.
        /// </summary>
        /// <value>
        /// The current time.
        /// </value>
        public string Day
        {
            get
            {
                return _day;
            }
            set
            {
                _day = value;
                OnPropertyChanged("Day");
            }
        }
        private string _day;


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
        /// Gets or sets a value indicating whether to override the current time.
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

        /// <summary>
        /// Gets or sets a value indicating whether races are delayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if races are delayed; otherwise, <c>false</c>.
        /// </value>
        public bool SetDelayTime
        {
            get
            {
                return _setDelayTime;
            }
            set
            {
                _setDelayTime = value;
                OnPropertyChanged("SetDelayTime");
            }
        }
        private bool _setDelayTime;

        /// <summary>
        /// Gets or sets the delay time.
        /// </summary>
        /// <value>
        /// The manual time.
        /// </value>
        public string DelayTime
        {
            get
            {
                return _delayTime;
            }
            set
            {
                _delayTime = value;
                OnPropertyChanged("DelayTime");
            }
        }
        private string _delayTime;

        /// <summary>
        /// Reduzierte Ansicht bei den Rennen (nur aktueller Tag und Zeitbereich)
        /// </summary>
        /// <value>
        /// The races reduced view.
        /// </value>
        public bool RacesReducedView
        {
            get
            {
                return _racesReducedView;
            }
            set
            {
                _racesReducedView = value;
                OnPropertyChanged("RacesReducedView");
            }
        }
        private bool _racesReducedView;
    }
}
