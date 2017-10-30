using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mrv.Regatta.Waage.Controls
{
    public class DescriptionButton : Button
    {

        /// <summary>
        /// Initializes the <see cref="DescriptionButton"/> class.
        /// </summary>
        static DescriptionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionButton), new FrameworkPropertyMetadata(typeof(DescriptionButton)));
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(DescriptionButton), new PropertyMetadata(null));
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(DescriptionButton), new PropertyMetadata(null));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

    }
}
