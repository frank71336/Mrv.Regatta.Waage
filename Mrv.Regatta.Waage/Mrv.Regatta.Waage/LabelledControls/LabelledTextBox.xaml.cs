using System.Windows;
using System.Windows.Controls;

namespace Mrv.Regatta.Waage.LabelledControls
{
    public partial class LabelledTextBox : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label",
                    typeof(string),
                    typeof(LabelledTextBox),
                    new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register("Text",
                    typeof(string),
                    typeof(LabelledTextBox),
                    new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelledTextBox"/> class.
        /// </summary>
        public LabelledTextBox()
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
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }

}
