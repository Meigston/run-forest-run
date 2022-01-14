namespace Brokers.RabbitMq
{
    using RabbitMQ.Client;

    internal class ConnectionFactories
    {
        public string ClientName { get; set; }

        public IConnectionFactory Default { get; set; }

        public IConnection DefaultConnection { get; set; }

        public IConnectionFactory AsyncFactory { get; set; }

        public IConnection AsyncConnection { get; set; }
    }
}
