using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Mrv.Regatta.Waage.LabelledControls
{
    public partial class LabelledComboBox : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label",
                    typeof(string),
                    typeof(LabelledComboBox),
                    new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty
            .Register("SelectedItem",
                    typeof(object),
                    typeof(LabelledComboBox),
                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty
            .Register("ItemsSource",
                    typeof(IEnumerable),
                    typeof(LabelledComboBox),
                    new FrameworkPropertyMetadata(new List<object>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelledComboBox"/> class.
        /// </summary>
        public LabelledComboBox()
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
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>
        /// The items source.
        /// </value>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
    }

}
