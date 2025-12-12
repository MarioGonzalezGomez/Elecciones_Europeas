namespace Elecciones.src.service
{
    public interface INotificationService
    {
        void ShowError(string message, string title);
        void ShowInfo(string message, string title);
        void ShowWarning(string message, string title);
    }
}
