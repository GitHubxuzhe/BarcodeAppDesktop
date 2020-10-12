using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BarcodeAppDesktop.Converter
{
    public class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (Visibility) value;
            if (v == Visibility.Collapsed || v == Visibility.Hidden)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (Visibility) value;
            if (v == Visibility.Collapsed || v == Visibility.Hidden)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }
    }
}