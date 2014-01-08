// Copyright (c) 2014, Dijji, and released under BSD, as defined by the text in the root of this distribution.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Johnson
{
    public class EnumMatchToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue,
                     StringComparison.InvariantCultureIgnoreCase) ? Visibility.Visible : Visibility.Collapsed; 
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            // One way conversion only
            return null;
        }
    }
}
