using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Markup;
using System.Xml;


namespace BarcodeAppDesktop.Helper
{
    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            string xaml = XamlWriter.Save(a);
            return (T)XamlReader.Load(new XmlTextReader(new StringReader(xaml)));
        }
    }
}
