using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ModCreator.Models;

namespace ModCreator.Converters
{
    public class RootItemToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventActionBase action)
            {
                // Hide button if it's root element or hidden item
                if (action.Name == Constants.EventActionRootElement.Name || action.IsHidden)
                    return Visibility.Collapsed;
            }
            
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
