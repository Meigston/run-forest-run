namespace Brokers.RabbitMq
{
    using Brokers.Common;

    public class BrokerProcessingErrorEvent : BaseEvent
    {
        public EventDetails Event { get; set; }

        public string ClientName { get; set; }

        public int ErrorCount { get; set; }

        public ExceptionDetails Exception { get; set; }
    }

    public class EventDetails
    {
        public string OriginalData { get; set; }

        public string AggregationId { get; set; }

        public string EventId { get; set; }

        public string EventType { get; set; }

        public string EventVersion { get; set; }
    }

    public class ExceptionDetails
    {
        public string Message { get; set; }

        public string Type { get; set; }

        public string StackTrace { get; set; }

        public ExceptionDetails InnerException { get; set; }
    }
}
