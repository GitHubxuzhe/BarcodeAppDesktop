using System;
using System.Globalization;
using System.Windows.Data;

namespace BarcodeAppDesktop.Converter
{
    internal class CultureToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((CultureInfo) (value)).TwoLetterISOLanguageName == parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return (bool) value
                ? (((string) parameter == "iv")
                    ? CultureInfo.InvariantCulture
                    : CultureInfo.GetCultureInfoByIetfLanguageTag(parameter.ToString()))
                : null;
        }
    }
}