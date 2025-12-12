using System.Windows;

namespace Elecciones
{
    /// <summary>
    /// Splash screen shown during application initialization
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the status text displayed on the splash screen
        /// </summary>
        /// <param name="status">Status message to display</param>
        public void UpdateStatus(string status)
        {
            if (Dispatcher.CheckAccess())
            {
                StatusText.Text = status;
            }
            else
            {
                Dispatcher.Invoke(() => StatusText.Text = status);
            }
        }
    }
}
