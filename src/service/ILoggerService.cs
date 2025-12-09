using System;

namespace Elecciones_Europeas.src.service
{
    public interface ILoggerService
    {
        void LogError(string message, Exception ex = null);
        void LogInfo(string message);
    }
}
