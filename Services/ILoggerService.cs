using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsServiceWrapper.Services
{
    public interface ILoggerService
    {
        void LogError(Exception ex);

        void LogError(string errorMessage);

        void LogInfo(string infoMessage);

        void LogWarn(Exception ex);

        void LogWarn(string warningMessage);

        void LogDebug(string debugMessage);
    }
}
