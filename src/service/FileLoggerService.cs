using System;
using System.IO;

namespace Elecciones.src.service
{
    public class FileLoggerService : ILoggerService
    {
        private static FileLoggerService _instance;
        private readonly string _logPath;

        private FileLoggerService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "Elecciones", "Logs");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            _logPath = Path.Combine(folder, $"log_{DateTime.Now:yyyyMMdd}.txt");
        }

        public static FileLoggerService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FileLoggerService();
            }
            return _instance;
        }

        public void LogError(string message, Exception ex = null)
        {
            string logMessage = $"[ERROR] {DateTime.Now:HH:mm:ss} - {message}";
            if (ex != null)
            {
                logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            WriteToFile(logMessage);
        }

        public void LogInfo(string message)
        {
            string logMessage = $"[INFO] {DateTime.Now:HH:mm:ss} - {message}";
            WriteToFile(logMessage);
        }

        private void WriteToFile(string message)
        {
            try
            {
                File.AppendAllText(_logPath, message + Environment.NewLine);
            }
            catch
            {
                // Si falla el log, no podemos hacer mucho más que ignorarlo para no tumbar la app
            }
        }
    }
}
