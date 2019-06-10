using System.Windows.Controls;

// https://www.markwithall.com/programming/2014/05/02/labelled-textbox-in-wpf.html

namespace Mrv.Regatta.Waage.LabelledControls
{
    public class LayoutGroup : StackPanel
    {
        public LayoutGroup()
        {
            Grid.SetIsSharedSizeScope(this, true);
        }
    }

}
