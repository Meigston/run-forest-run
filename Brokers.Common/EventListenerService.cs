namespace Brokers.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Brokers.Common.Interfaces;

    using Microsoft.Extensions.Hosting;

    public class EventListenerService : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly IEventSubscriber _eventListener;

        public EventListenerService(IEventSubscriber eventListener)
        {
            _eventListener = eventListener;
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            var task = new Task(async () =>
                {
                    _eventListener.StartListener();
                    do
                    {
                        await Task.Delay(1000, _stoppingCts.Token);
                    } while (!_stoppingCts.IsCancellationRequested);
                });

            task.Start();
            _executingTask = task;
            return task;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public virtual void Dispose()
        {
            (_eventListener as IDisposable).Dispose();
            _stoppingCts.Cancel();
        }
    }
}
