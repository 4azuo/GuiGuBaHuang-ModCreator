using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts multiple string values to Visibility.
    /// Returns Visible if first value equals second value.
    /// Usage: First binding is the value to compare, second binding is the expected value.
    /// </summary>
    public class StringEqualityToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return Visibility.Collapsed;

            var value1 = values[0]?.ToString() ?? string.Empty;
            var value2 = values[1]?.ToString() ?? string.Empty;

            return value1.Equals(value2, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
