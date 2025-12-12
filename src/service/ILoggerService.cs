using System;

namespace Elecciones.src.service
{
    public interface ILoggerService
    {
        void LogError(string message, Exception ex = null);
        void LogInfo(string message);
    }
}
