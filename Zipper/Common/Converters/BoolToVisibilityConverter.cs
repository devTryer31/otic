using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Zipper.Common.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value switch
            {
                Visibility.Visible => true,
                Visibility.Hidden => false,
                Visibility.Collapsed => (object)false,
                _ => throw new IndexOutOfRangeException(nameof(value)),
            };
        }
    }
}