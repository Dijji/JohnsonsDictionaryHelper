// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System.Windows.Controls;
using System.Windows.Input;

namespace Johnson
{
    // Custom command definitions
    public static class Command
    {
        public static RoutedCommand Italicise = new RoutedCommand("Italicise", typeof(TextBox));
        public static RoutedCommand AEUpper = new RoutedCommand("AEUpper", typeof(TextBox));
        public static RoutedCommand AELower = new RoutedCommand("AELower", typeof(TextBox));
        public static RoutedCommand OEUpper = new RoutedCommand("OEUpper", typeof(TextBox));
        public static RoutedCommand OELower = new RoutedCommand("OELower", typeof(TextBox));
        public static RoutedCommand Number = new RoutedCommand("Number", typeof(TextBox));
        public static RoutedCommand Section = new RoutedCommand("Section", typeof(TextBox));
        public static RoutedCommand Dash = new RoutedCommand("Dash", typeof(TextBox));
        public static RoutedCommand Space = new RoutedCommand("Space", typeof(TextBox));
        public static RoutedCommand RemoveBreaks = new RoutedCommand("RemoveBreaks", typeof(TextBox));
        public static RoutedCommand GrowFont = new RoutedCommand("GrowFont", typeof(TextBox));
        public static RoutedCommand ShrinkFont = new RoutedCommand("ShrinkFont", typeof(TextBox));
    }
}
