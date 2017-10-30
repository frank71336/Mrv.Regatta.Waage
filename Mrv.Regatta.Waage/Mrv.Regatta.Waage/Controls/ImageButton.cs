using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mrv.Regatta.Waage.Controls
{
    public class ImageButton : Button
    {
        /// <summary>
        /// Initializes the <see cref="ImageButton"/> class.
        /// </summary>
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));
        public static readonly DependencyProperty ImagePositionProperty = DependencyProperty.Register("ImagePosition", typeof(ImagePositions), typeof(ImageButton), new UIPropertyMetadata(null));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public ImagePositions ImagePosition
        {
            get { return (ImagePositions)GetValue(ImagePositionProperty); }
            set { SetValue(ImagePositionProperty, value); }
        }

        public enum ImagePositions
        {
            Top,
            Left,
            Right,
            Bottom,
        }

    }
}
