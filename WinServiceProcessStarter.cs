using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsServiceWrapper.Services;

namespace WindowsServiceWrapper
{
    public class WinServiceProcessStarter : IHostedService, IDisposable
    {
        private ILoggerService logger;
        private IConfigReader configReader;

        public WinServiceProcessStarter(ILoggerService logger, IConfigReader configReader)
        {
            this.logger = logger;
            this.configReader = configReader;
        }

        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            logger.LogInfo("Starting application as service!");

            Process process = new Process();
            process.StartInfo.FileName = configReader.GetValue("AppToStart");

            var variables = configReader.ConfigValues;

            logger.LogInfo($"Variales count is: {variables.Count()}" );

            foreach (var item in variables)
            {
                if (item.Value != null)
                    process.StartInfo.EnvironmentVariables[item.Key] = item.Value;
            }

            process.StartInfo.Arguments = configReader.GetValue("StartupFile");

            logger.LogInfo($"Process arguments: {process.StartInfo.Arguments}");

            logger.LogInfo("Process starting...");

            process.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private string FindMainJsFile(string jsFileName)
        {
            var tempDirectory = AppDomain.CurrentDomain.BaseDirectory;

            logger.LogInfo($"CurrentDirectory is: {tempDirectory}");

            while (Directory.Exists(tempDirectory))
            {
                var filePaths = Directory.GetFiles(tempDirectory);

                foreach (var filePath in filePaths)
                {
                    if (Path.GetFileName(filePath) == jsFileName)
                    {
                        logger.LogInfo($"Startup JS file found: {filePath}");
                        return filePath;
                    }
                }

                tempDirectory = Directory.GetParent(tempDirectory).FullName;
            }

            throw new FileNotFoundException($"Sms sender main javascript file \"{jsFileName}\" not found!");
        }

    }
}
