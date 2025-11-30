using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts element type to Visibility for ComboBox (type = "combo" = Visible)
    /// </summary>
    public class ComboBoxVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type.Equals("combo", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
