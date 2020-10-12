using BarcodeAppDesktop.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace BarcodeAppDesktop.Converter
{
    public class PrecodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strField = value as string;
            
            if (!Settings.Default.EnableLabelPrefix)
                return strField.ToUpper();

            var strPreCode = parameter as string;
            if (!String.IsNullOrEmpty(strField) && !String.IsNullOrEmpty(strPreCode))
            {
                return !strField.StartsWith(strPreCode) ? (strPreCode + strField).ToUpper() : strField.ToUpper();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}