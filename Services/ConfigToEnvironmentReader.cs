using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsServiceWrapper.Services
{
    public class ConfigToEnvironmentReader : IConfigReader
    {
        public IDictionary<string, string> ConfigValues => configValues;

        private Logger logger = LogManager.GetLogger("common");
        private Dictionary<string, string> configValues;
        public ConfigToEnvironmentReader()
        {
            configValues = new Dictionary<string, string>();
            ReadConfig(AppDomain.CurrentDomain.BaseDirectory);
            LoadEnvironmentVariablesFromConfig();
        }

        public string GetValue(string key)
        {
            return ConfigValues[key];
        }

        public void LoadEnvironmentVariablesFromConfig()
        {
            logger.Debug($"Loading env variables from config started. ConfigValues count is {ConfigValues.Count}");
            foreach (var item in ConfigValues)
            {
                logger.Debug($"Loading env variables from config- Key: {item.Key} Value: {item.Value}");
                Environment.SetEnvironmentVariable(item.Key, item.Value);
            }
        }

        public bool ReadConfig(string pathToConfig)
        {
            if (string.IsNullOrWhiteSpace(pathToConfig))
                throw new ArgumentNullException(nameof(pathToConfig));

            try
            {
                var config = new ConfigurationBuilder()
                         .SetBasePath(pathToConfig)
                         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                         .Build();

                if (config == null)
                    throw new ArgumentNullException($"Couldn't read appsettings.json from this location: {pathToConfig}");
                
                var appsToWrap = config.GetSection("AppsToWrap").GetChildren().ToList();

                var appToStart = appsToWrap.FirstOrDefault(app => app.GetValue<bool>("IsAppMustBeWrapped"));

                if (appToStart == null)
                    throw new Exception("Please specify in appsettings.json which application must be wrapped. Set \"IsAppMustBeWrapped\" property to true!");

                logger.Debug($"Path to config file: {config.GetValue<string>("NLOG_CONFIG_FILE")}");
                logger.Debug($"Path to logs: {config.GetValue<string>("NLOG_LOGS_PATH")}");

                ConfigValues.Add("NLOG_CONFIG_FILE", config.GetValue<string>("NLOG_CONFIG_FILE"));
                ConfigValues.Add("NLOG_LOGS_PATH", config.GetValue<string>("NLOG_LOGS_PATH"));
                ConfigValues.Add("AppToStart", appToStart.GetValue<string>("AppToStart"));
                ConfigValues.Add("StartupFile", appToStart.GetValue<string>("StartupFile"));

                var variables = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") == "Production" ?
                    appToStart.GetSection("Production").GetChildren() : appToStart.GetSection("Development").GetChildren();

                logger.Debug($"Variables count: {variables.Count()}");

                foreach (var item in variables)
                {
                    if (item.Value != null) {
                        logger.Debug($"Key: {item.Key} Value: {item.Value}");
                        ConfigValues.Add(item.Key, item.Value);
                    }
                }

                logger.Debug($"Config values count after adding: {ConfigValues.Count}");
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }
    }
}
