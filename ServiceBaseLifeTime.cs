using Microsoft.Extensions.Hosting;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using WindowsServiceWrapper.Services;

namespace WindowsServiceWrapper
{
    public class ServiceBaseLifeTime : ServiceBase, IHostLifetime
    {
        private readonly TaskCompletionSource<object> _delayStart;
        private IApplicationLifetime ApplicationLifetime { get; }

        private ILoggerService logger;

        public ServiceBaseLifeTime(IApplicationLifetime applicationLifetime, ILoggerService logger)
        {
            _delayStart = new TaskCompletionSource<object>();
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => _delayStart.TrySetCanceled());
            ApplicationLifetime.ApplicationStopping.Register(Stop);

            new Thread(Run).Start();

            return _delayStart.Task;
        }


        private void Run()
        {
            try
            {
                Run(this);
                _delayStart.TrySetException(new InvalidOperationException("Service stopped without starting!"));
            }
            catch (Exception ex)
            {
                _delayStart.TrySetException(ex);
            }
        }

        protected override void OnStart(string[] args)
        {
            _delayStart.TrySetResult(null);
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            ApplicationLifetime.StopApplication();
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }
    }
}
