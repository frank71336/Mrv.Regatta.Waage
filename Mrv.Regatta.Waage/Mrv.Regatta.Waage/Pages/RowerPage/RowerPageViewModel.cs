using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Mrv.Regatta.Waage.Pages.RowerPage
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ViewModelBase.ViewModelBase" />
    class RowerPageViewModel : ViewModelBase.ViewModelBase
    {
        public Gender Gender { get; set; }
        public string Name { get; set; }
        public string Club { get; set; }
        public string YearOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
                OnPropertyChanged("Comment");
            }
        }
        private string _comment { get; set; }

        public Brush CommentBrush
        {
            get
            {
                return _commentBrush;
            }
            set
            {
                _commentBrush = value;
                OnPropertyChanged("CommentBrush");
            }
        }
        private Brush _commentBrush { get; set; }

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

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>
        /// The weightings.
        /// </value>
        public ObservableCollection<Weighting> Weightings
        {
            get
            {
                return _weightings;
            }
            set
            {
                _weightings = value;
                OnPropertyChanged("Weightings");
            }
        }
        private ObservableCollection<Weighting> _weightings;

        public class Weighting
        {
            public string Mass { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
        }

    }
}
