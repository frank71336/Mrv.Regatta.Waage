using System.Collections.ObjectModel;

namespace Mrv.Regatta.Waage.Pages.RacesPage
{
    class RacesPageViewModel : ViewModelBase.ViewModelBase
    {
        /// <summary>
        /// Gets or sets the races.
        /// </summary>
        /// <value>
        /// The races.
        /// </value>
        public ObservableCollection<UserControls.Race> Races
        {
            get
            {
                return _races;
            }
            set
            {
                _races = value;
                OnPropertyChanged("Races");
            }
        }
        private ObservableCollection<UserControls.Race> _races;
    }
}
