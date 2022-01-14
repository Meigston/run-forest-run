namespace Brokers.RabbitMq
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMqHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider serviceProvider;

        public RabbitMqHealthCheck(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return await Task.Run<HealthCheckResult>(HealthCheckSync, cancellationToken);
        }

        private HealthCheckResult HealthCheckSync()
        {
            var state = HealthCheckState.Init;
            try
            {
                using (var channel = this.serviceProvider.GetService<ConnectionFactories>().DefaultConnection.CreateModel())
                {
                    state = HealthCheckState.Connected;
                    var guid = Guid.NewGuid();
                    var exchangeName = $"ex-health-{guid}";
                    var queueName = $"queue-health-{guid}";
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, false, false);
                    state = HealthCheckState.ExchangeCreated;
                    channel.QueueDeclare(queueName, true, true, false);
                    state = HealthCheckState.QueueCreated;

                    try
                    {
                        channel.QueueBind(queueName, exchangeName, routingKey: "PING");
                        state = HealthCheckState.Bind;

                        var consumer = new AsyncEventingBasicConsumer(channel);

                        consumer.Received += async (sender, args) =>
                            {
                                await Task.Run(
                                    () =>
                                        {
                                            var str = Encoding.Default.GetString(args.Body.ToArray());
                                            if (string.Equals(str, guid.ToString()))
                                            {
                                                state = HealthCheckState.Receive;
                                            }
                                        });
                            };

                        channel.BasicConsume(queue: queueName, consumer: consumer);
                        state = HealthCheckState.Subscribe;

                        var date = DateTime.Now;
                        var testTime = TimeSpan.FromSeconds(5);
                        channel.BasicPublish(exchangeName, "PING", null, Encoding.Default.GetBytes(guid.ToString()));
                        state = HealthCheckState.Send;

                        do
                        {
                            Thread.Sleep(100);
                        }
                        while (state != HealthCheckState.Receive && (DateTime.Now - date) <= testTime);

                        if (state != HealthCheckState.Receive)
                        {
                            return HealthCheckResult.Unhealthy($"Consumer time is over");
                        }
                    }
                    finally
                    {
                        channel.ExchangeDeleteNoWait(exchangeName);
                        channel.QueueDeleteNoWait(queueName);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error/ Current state {state}";

                return HealthCheckResult.Unhealthy($"{error}", ex);
            }

            return HealthCheckResult.Healthy();
        }

        protected enum HealthCheckState
        {
            Init = 0,
            Connected,
            ExchangeCreated,
            QueueCreated,
            Bind,
            Subscribe,
            Send,
            Receive
        }
    }
}
