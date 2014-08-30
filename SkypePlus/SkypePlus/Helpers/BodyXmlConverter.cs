using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace SkypePlus.Helpers
{
    public class BodyXmlConverter : IValueConverter
    {
        private Regex _linkRegex = new Regex(@"\[a\s+href='(?<link>[^']+)'\](?<text>.*?)\[/a\]", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            //if (String.IsNullOrEmpty(text)) return "";

            ////var formattedInlines = new InlineCollection();

            ////foreach (var match in _linkRegex.Matches(text))
            ////{
                
            ////}

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
