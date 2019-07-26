using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowsServiceWrapper.Services;

namespace WindowsServiceWrapper
{
    class Program
    {
        private static Logger logger = LogManager.GetLogger("common");
        static async Task Main(string[] args)
        {
            try
            {

                logger.Debug("In main method...");

                bool isAsServiceStarted = !(Debugger.IsAttached || args.Contains("--console"));

                var builder = new HostBuilder().ConfigureServices((hostContext, services) => {
                    services.AddSingleton<IConfigReader, ConfigToEnvironmentReader>();
                    services.AddSingleton<ILoggerService, NlogLogger>();
                    services.AddHostedService<WinServiceProcessStarter>();
                });

                builder.UseEnvironment(isAsServiceStarted ? EnvironmentName.Production : EnvironmentName.Development);

                if (isAsServiceStarted)
                {
                    logger.Debug("Service started in Production mode");
                    Environment.SetEnvironmentVariable("NETCORE_ENVIRONMENT", "Production");
                    await builder.RunAsServiceAsync();
                }
                else
                {
                    logger.Debug("Service started in Development mode");
                    Environment.SetEnvironmentVariable("NETCORE_ENVIRONMENT", "Development");
                    await builder.RunConsoleAsync();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.Write(ex.Message);
            }
        }
    }
}
