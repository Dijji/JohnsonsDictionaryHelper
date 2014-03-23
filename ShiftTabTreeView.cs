using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Johnson
{
    public class ShiftTabTreeView : TreeView
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 && e.Key == Key.Tab)
                return;

            base.OnKeyDown(e);
        }
    }
}
