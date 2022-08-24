using System;
using System.Globalization;
using System.Windows;

namespace SmartInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            SetLanguageDictionary();
        }

        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "en":
                    dict.Source = new Uri("..\\Languages\\StringResources-EN.xaml", UriKind.Relative);
                    break;
                    
                case "fr":
                    dict.Source = new Uri("..\\Languages\\StringResources-FR.xaml", UriKind.Relative);
                    break;
                    
                case "ar":
                    dict.Source = new Uri("..\\Languages\\StringResources-AR.xaml", UriKind.Relative);
                    break;
                    
                case "es":
                    dict.Source = new Uri("..\\Languages\\StringResources-ES.xaml", UriKind.Relative);
                    break;
                    
                case "de":
                    dict.Source = new Uri("..\\Languages\\StringResources-DE.xaml", UriKind.Relative);
                    break;
                    
                case "ru":
                    dict.Source = new Uri("..\\Languages\\StringResources-RU.xaml", UriKind.Relative);
                    break;
                    
                case "tr":
                    dict.Source = new Uri("..\\Languages\\StringResources-TR.xaml", UriKind.Relative);
                    break;
                    
                case "pt":
                    dict.Source = new Uri("..\\Languages\\StringResources-PT.xaml", UriKind.Relative);
                    break;
                    
                default:
                    dict.Source = new Uri("..\\Languages\\StringResources-EN.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
