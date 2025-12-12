using System.Configuration;
using System.Data;
using System.Windows;

namespace Elecciones
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
            
            // Global exception handling
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            var themeUri = new Uri("styles/Light.xaml", UriKind.Relative);
            CambiarTheme(themeUri);

            // Show splash screen during initialization
            var splash = new SplashWindow();
            splash.Show();

            //Allow splash to render
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Action(delegate { }));

            try
            {
                splash.UpdateStatus("Inicializando aplicación...");
                
                // Create and show main window
                var mainWindow = new MainWindow();
                this.MainWindow = mainWindow;
                
                splash.UpdateStatus("Aplicación lista");
                System.Threading.Thread.Sleep(500); // Brief pause to show success message
                
                splash.Close();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                splash.Close();
                
                var logger = Elecciones.src.service.FileLoggerService.GetInstance();
                logger.LogError("Failed to initialize application", ex);

                var notificationService = Elecciones.src.service.NotificationService.GetInstance();
                notificationService.ShowError(
                    $"No se pudo iniciar la aplicación debido a un error de conexión.\n\n" +
                    $"Por favor, verifique:\n" +
                    $"• Que los servidores de base de datos están activos\n" +
                    $"• Que la configuración de red es correcta\n" +
                    $"• Que las credenciales son válidas\n\n" +
                    $"ERROR: {ex.Message}\n\n" +
                    $"Presione OK para cerrar.",
                    "Error de Inicio"
                );

                // TEMPORALMENTE COMENTADO PARA DEBUGGING
                // Environment.Exit(1);
                this.Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = Elecciones.src.service.FileLoggerService.GetInstance();
            logger.LogError("An unhandled exception occurred.", e.Exception);

            var notificationService = Elecciones.src.service.NotificationService.GetInstance();
            notificationService.ShowError($"Ha ocurrido un error inesperado: {e.Exception.Message}", "Error Crítico");

            e.Handled = true; // Prevent crash if possible, or let it crash after logging if preferred. 
            // Usually for critical errors we might want to let it crash or shutdown gracefully.
            // For now, keeping it alive might be safer for the user experience if it's a minor UI glitch.
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
