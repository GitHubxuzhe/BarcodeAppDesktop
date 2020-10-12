using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using BarcodeAppDesktop.Properties;

namespace BarcodeAppDesktop.Converter
{
    class TestServerToBooleanConverter : MarkupExtension, IValueConverter
    {
        private static TestServerToBooleanConverter _converter = null;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mhsServerAdress = (string)value;
            var testIps = Settings.Default.TestServerIps;
            var splitIps = testIps.Split(';');
            foreach (string ip in splitIps)
            {
                if (mhsServerAdress.Equals(ip.Trim()))
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new TestServerToBooleanConverter());
        }
    }
}
