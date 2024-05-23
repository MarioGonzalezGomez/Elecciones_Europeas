using System.Configuration;
using System.Data;
using System.Windows;

namespace Elecciones_Europeas
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Lógica para aplicar el theme al iniciar la aplicación
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var themeUri = new Uri("styles/Light.xaml", UriKind.Relative);
            CambiarTheme(themeUri);
        }

        public void CambiarTheme(Uri uri)
        {
            var resourceDict = new ResourceDictionary();
            resourceDict.Source = uri;

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
    }

}
