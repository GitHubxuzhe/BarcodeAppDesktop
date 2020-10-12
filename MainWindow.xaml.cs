using System.Reflection;
using System.Windows;
using BarcodeAppDesktop.Properties;
using Telerik.Windows.Controls;

namespace BarcodeAppDesktop.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            StyleManager.ApplicationTheme = new Windows8Theme();
            InitializeComponent();

            this.Title += (string.Format(" - Version: {0}", Assembly.GetExecutingAssembly().GetName().Version));
            
            if(Settings.Default.EnableMaximizedWindowOnStartup)
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }
    }
}