using System.Globalization;
using System.Windows;
using BarcodeAppDesktop.Enumrations;
using BarcodeAppDesktop.Helper;
using BarcodeAppDesktop.Properties;
using Exceptionless;
using GalaSoft.MvvmLight.Messaging;
using WPFLocalizeExtension.Engine;

namespace BarcodeAppDesktop
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Overrides of Application

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExceptionlessClient.Current.Register();
            LocalizeDictionary.Instance.Culture =
                CultureInfo.GetCultureInfo(Settings.Default.SelectedLanguage.GetDescription());
            LocalizeDictionary.Instance.PropertyChanged +=
                (s, args) => Messenger.Default.Send(LocalizeDictionary.Instance.Culture);
        }


        #region Overrides of Application

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.SelectedLanguage =
                LocalizeDictionary.Instance.Culture.TwoLetterISOLanguageName.GetValueFromDescription<LanguageEnum>();
            Settings.Default.Save();
            base.OnExit(e);
        }

        #endregion

        #endregion
    }
}