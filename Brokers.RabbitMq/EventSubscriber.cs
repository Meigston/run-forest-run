namespace Brokers.RabbitMq
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;

    using Brokers.Common;
    using Brokers.Common.Interfaces;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class EventSubscriber : IEventSubscriber
    {
        private readonly ILogger _logger;
        private readonly RabbitMqOptions _rabbitMqOptions;

        internal IServiceScopeFactory ServiceScopeFactory { get; set; }

        internal IModel Channel { get; set; }

        public EventSubscriber(IConnection connection,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<EventSubscriber> logger,
            RabbitMqOptions rabbitMqOptions)
        {
            ServiceScopeFactory = serviceScopeFactory;
            Channel = connection.CreateModel();
            _logger = logger;
            _rabbitMqOptions = rabbitMqOptions;
        }

        public void StartListener()
        {
            var queue = Channel.ConfigureListenerQueue();
            var consumer = new AsyncEventingBasicConsumer(Channel);

            consumer.Received += OnEventReceived;

            Channel.BasicQos(_rabbitMqOptions.QosPrefetchSize, _rabbitMqOptions.QosPrefetchCount, _rabbitMqOptions.QosGlobal);
            Channel.BasicConsume(queue: queue, consumer: consumer);
        }

        private async Task OnEventReceived(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var ret = await OnConsumerOnReceived(ea);

                if (ret.Success)
                {
                    AckEvent(ea);
                }
                else
                {
                    NackEvent(ea);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on ConsumeEvent");
                throw;
            }
        }

        private void AckEvent(BasicDeliverEventArgs ea)
        {
            Channel.BasicAck(ea.DeliveryTag, false);
        }

        private void NackEvent(BasicDeliverEventArgs ea)
        {
            Channel.BasicNack(ea.DeliveryTag, false, true);
        }

        private async Task<ConsumeResult> OnConsumerOnReceived(BasicDeliverEventArgs ea)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                try
                {
                    return await ConsumeEvent(ea, scope);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error on OnConsumerOnReceived");
                    return e;
                }
            }
        }

        private async Task<bool> ConsumeEvent(BasicDeliverEventArgs ea, IServiceScope scope)
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            var type = Thread.GetDomain().GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(b => b.FullName == ea.RoutingKey))
                .FirstOrDefault();

            var service = scope.ServiceProvider.GetConsumerService(type);
            var deserializeObject = JsonConvert.DeserializeObject(message, type);

            return await service.Consume(deserializeObject as BaseEvent);
        }

        internal class ConsumeResult
        {
            public bool Success { get; set; }

            public ExceptionDetails Exception { get; set; }

            public static implicit operator ConsumeResult(bool success)
            {
                return new ConsumeResult()
                {
                    Success = success,
                    Exception = null
                };
            }

            public static implicit operator ConsumeResult(Exception exception)
            {
                return new ConsumeResult()
                {
                    Success = false,
                    Exception = GetExceptionDetails(exception)
                };
            }

            private static ExceptionDetails GetExceptionDetails(Exception ex)
            {
                return new ExceptionDetails()
                {
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException == null
                        ? null
                        : GetExceptionDetails(ex.InnerException)
                };
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Channel.IsOpen)
            {
                Channel.Close();
            }
        }
    }
}
