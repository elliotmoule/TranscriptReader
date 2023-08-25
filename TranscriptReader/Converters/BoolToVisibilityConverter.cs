using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TranscriptReader.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase);
            return value is bool b && b ? (invert ? Visibility.Collapsed : Visibility.Visible) : (invert ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }
}
