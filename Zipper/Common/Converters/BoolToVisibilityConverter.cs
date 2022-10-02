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
            //Если в parameter передается false, то нужно инвертировать входные значения.
            bool input = (bool)value;
            if(parameter is not null && !parameter.Equals("true"))
                input = !input;

            if (input)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ans = (Visibility)value switch
            {
                Visibility.Visible => true,
                Visibility.Hidden => false,
                Visibility.Collapsed => false,
                _ => throw new IndexOutOfRangeException(nameof(value)),
            };

            if (parameter is not null && !parameter.Equals("true"))
                ans = !ans;

            return ans;
        }
    }
}