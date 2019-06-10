using System;
using System.Windows;
using System.Windows.Controls;

namespace Mrv.Regatta.Waage.LabelledControls
{
    public partial class LabelledDatePicker : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label",
                    typeof(string),
                    typeof(LabelledDatePicker),
                    new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty
            .Register("SelectedDate",
                    typeof(DateTime),
                    typeof(LabelledDatePicker),
                    new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelledDatePicker"/> class.
        /// </summary>
        public LabelledDatePicker()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime SelectedDate
        {
            get { return (DateTime)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }
    }

}
