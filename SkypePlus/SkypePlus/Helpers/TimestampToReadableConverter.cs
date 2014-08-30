using System;
using System.Globalization;
using System.Windows.Data;

namespace SkypePlus.Helpers
{
    public class TimestampToReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) throw new ArgumentException("Value must be of type int.", "value");

            var timestamp = (int)value;
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(timestamp).ToLocalTime();

            return dateTime.ToString(dateTime < DateTime.Now.AddDays(-1) ? "g" : "t");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
