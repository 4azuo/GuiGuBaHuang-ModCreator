using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts string to Visibility (non-empty = Visible, empty/null = Collapsed)
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
