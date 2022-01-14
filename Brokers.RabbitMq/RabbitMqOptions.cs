namespace Brokers.RabbitMq
{
    using System;

    public class RabbitMqOptions
    {
        public RabbitMqOptions(string hostName, int? port, string userName, string passWord, string clientName = "", ushort qosPrefetchSize = 10)
        {
            this.HostName = hostName;
            this.Port = port;
            this.UserName = userName;
            this.PassWord = passWord;
            this.CleanAttemptsPeriod = TimeSpan.FromMinutes(2);
            this.CleanAfterPeriod = TimeSpan.FromMinutes(2);
            this.QosPrefetchCount = qosPrefetchSize;
            this.QosGlobal = false;
            this.ClientName = clientName;
        }

        public RabbitMqOptions(string clientName = "", ushort qosPrefetchSize = 10)
        {
            this.CleanAttemptsPeriod = TimeSpan.FromMinutes(2);
            this.CleanAfterPeriod = TimeSpan.FromMinutes(2);
            this.QosPrefetchCount = qosPrefetchSize;
            this.QosGlobal = false;
            this.ClientName = clientName;
        }

        public string Uri { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string HostName { get; set; }

        public string VirtualHost { get; set; } = string.Empty;

        public int? Port { get; set; }

        public string ClientName { get; set; }

        public uint QosPrefetchSize { get; set; } = default;

        public ushort QosPrefetchCount { get; set; }

        public bool QosGlobal { get; set; }

        public int? MaxErrorCount { get; set; }

        public TimeSpan CleanAttemptsPeriod { get; set; }

        public TimeSpan CleanAfterPeriod { get; set; }
    }
}
