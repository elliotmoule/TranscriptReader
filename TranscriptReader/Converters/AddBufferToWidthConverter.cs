using System;
using System.Globalization;
using System.Windows.Data;

namespace TranscriptReader.Converters
{
    public class AddBufferToWidthConverter : IValueConverter
    {
        private readonly double _buffer = 70;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retVal = value is double d ? d - _buffer : value;
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double d ? d + _buffer : value;
        }
    }
}
