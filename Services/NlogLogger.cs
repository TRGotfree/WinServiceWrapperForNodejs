using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsServiceWrapper.Services
{

    public class NlogLogger : ILoggerService
    {
        private Logger logger = LogManager.GetLogger("common");
        private IConfigReader configReader;

        public NlogLogger(IConfigReader configReader)
        {
            this.configReader = configReader;

            logger.Debug("NLogLogger service constructor started...");

            string pathToNlogConfigFile = Environment.GetEnvironmentVariable("NLOG_CONFIG_FILE") ?? throw new ArgumentNullException(nameof(pathToNlogConfigFile));
            string pathToLogs = Environment.GetEnvironmentVariable("NLOG_LOGS_PATH") ?? throw new ArgumentNullException(nameof(pathToLogs));

            logger.Debug("pathToLogs: {0}", pathToLogs);
            logger.Debug("pathToConfigFile: {0}", pathToNlogConfigFile);

            LogManager.LoadConfiguration(pathToNlogConfigFile);
            LogManager.Configuration.Variables["logsDirectory"] = pathToLogs;

            logger = LogManager.GetLogger("WindowsServiceWrapper");
        }
         
        public void LogDebug(string debugMessage)
        {
            logger.Debug(debugMessage);
        }

        public void LogError(Exception ex)
        {
            logger.Error(ex);
        }

        public void LogError(string errorMessage)
        {
            logger.Error(errorMessage);
        }

        public void LogInfo(string infoMessage)
        {
            logger.Info(infoMessage);
        }

        public void LogWarn(Exception ex)
        {
            logger.Warn(ex);
        }

        public void LogWarn(string warningMessage)
        {
            logger.Warn(warningMessage);
        }
    }
}
