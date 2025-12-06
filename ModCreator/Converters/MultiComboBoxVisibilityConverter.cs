using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts element type to Visibility for MultiSelectComboBox (type = "multicombo" = Visible)
    /// </summary>
    public class MultiComboBoxVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type.Equals("multicombo", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
