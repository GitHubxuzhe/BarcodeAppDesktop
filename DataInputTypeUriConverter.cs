using System;
using System.Globalization;
using System.Windows.Data;
using BarcodeAppDesktop.Enumrations;
using BarcodeAppDesktop.Properties;

namespace BarcodeAppDesktop.Converter
{
    public class DataInputTypeUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataInputType = (DataInputSourceEnum) value;

            if(!Settings.Default.IsInternal)
                return new Uri("/BarcodeAppDesktop;component/View/ManualInput.xaml", UriKind.Relative);

            switch (dataInputType)
            {
                case DataInputSourceEnum.Import:
                    return new Uri("/BarcodeAppDesktop;component/View/ImportData.xaml", UriKind.Relative);
                case DataInputSourceEnum.Manual:
                    return new Uri("/BarcodeAppDesktop;component/View/ManualInput.xaml", UriKind.Relative);
                case DataInputSourceEnum.ContainerScan:
                    return new Uri("/BarcodeAppDesktop;component/View/ContainerScan.xaml", UriKind.Relative);
                case DataInputSourceEnum.ContainerSearch:
                    return new Uri("/BarcodeAppDesktop;component/View/ContainerSearch.xaml", UriKind.Relative);
            }
            return new Uri("/BarcodeAppDesktop;component/View/ManualInput.xaml", UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}