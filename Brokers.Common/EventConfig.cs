namespace Brokers.Common
{
    using System;

    public class EventConfig
    {
        public string EventId { get; set; }

        public string Token { get; set; }

        public DateTime Date { get; set; }

        public string EventType { get; set; }

        public string HistoryIds { get; set; }

        public string HistoryTypes { get; set; }

        public string Delayed { get; set; }
    }
}
