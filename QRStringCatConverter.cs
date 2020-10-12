using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BarcodeAppDesktop.Converter
{
    public class QrStringCatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = values.Cast<string>().Aggregate(string.Empty, (current, str) => current + '$' + str);
            result = "QR" + result;
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}