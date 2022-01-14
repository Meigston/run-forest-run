namespace Brokers.RabbitMq
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Brokers.Common.Interfaces;

    using Microsoft.Extensions.Logging;

    using RabbitMQ.Client;

    public class RawEventPublisher : IRawEventPublisher, IDisposable
    {
        private readonly ILogger logger;

        internal IModel Channel { get; set; }

        public RawEventPublisher(IConnection connection, ILogger<EventPublisher> logger)
        {
            Channel = connection.CreateModel();
            this.logger = logger;
        }

        public async Task SendRaw(string eventBody,
            string routingKey,
            string exchange,
            string messageId = null,
            string correlationId = null,
            Dictionary<string, object> additionalHeaders = null)
        {
            var body = Encoding.UTF8.GetBytes(eventBody);

            await Task.Run(() =>
            {
                Channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
            });
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
