using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebStore.Infrastructure.Services
{
    public class TestHostedService : IHostedService
    {
        private readonly ILogger<TestHostedService> _Logger;
        private CancellationTokenSource _Cancellation;
        private const int __Timeout = 3000;

        public TestHostedService(ILogger<TestHostedService> Logger) => _Logger = Logger;

        public Task StartAsync(CancellationToken Cancel)
        {
            _Logger.LogInformation("Test service starting");

            var cancellation = new CancellationTokenSource();
            Interlocked.Exchange(ref _Cancellation, cancellation)?.Cancel();

            StartWork(cancellation.Token);

            _Logger.LogInformation("Test service started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken Cancel)
        {
            _Logger.LogInformation("Test service stopping");

            _Cancellation?.Cancel();

            _Logger.LogInformation("Test service stopped");
            return Task.CompletedTask;
        }

        private async void StartWork(CancellationToken Cancel)
        {
            try
            {
                await Task.Run(() => Work(Cancel), Cancel);
            }
            catch (OperationCanceledException)
            {
                _Logger.LogInformation("Работа службы завершена");
            }
        }

        private async Task Work(CancellationToken Cancel)
        {
            do
            {
                Cancel.ThrowIfCancellationRequested();
                _Logger.LogInformation("Test service do work...");
                await Task.Delay(__Timeout, Cancel).ConfigureAwait(false);
            }
            while (!Cancel.IsCancellationRequested);
            Cancel.ThrowIfCancellationRequested();
        }
    }
}
