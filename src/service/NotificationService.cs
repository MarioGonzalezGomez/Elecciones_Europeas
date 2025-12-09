using System.Windows;

namespace Elecciones_Europeas.src.service
{
    public class NotificationService : INotificationService
    {
        private static NotificationService _instance;

        public static NotificationService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new NotificationService();
            }
            return _instance;
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInfo(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
