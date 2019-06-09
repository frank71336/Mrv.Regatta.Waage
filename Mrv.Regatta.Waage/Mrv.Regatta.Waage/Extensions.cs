using System.Windows;

namespace Mrv.Regatta.Waage
{
    public static class Extensions
    {
        #region Id-Property für WPF-Controls

        // http://stackoverflow.com/questions/18108648/wpf-adding-a-custom-property-in-a-control
        // http://www.c-sharpcorner.com/UploadFile/vikie4u/attached-properties-in-wpf/

        /// <summary>
        /// The identifier property
        /// </summary>
        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached("Id", typeof(int), typeof(Extensions), new PropertyMetadata(default(int)));

        /// <summary>
        /// Sets the identifier.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        public static void SetId(UIElement element, int value)
        {
            element.SetValue(IdProperty, value);
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static int GetId(UIElement element)
        {
            return (int)element.GetValue(IdProperty);
        }

        #endregion
    }

}
