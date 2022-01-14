namespace Brokers.Common
{
    using System;

    public class EventAssignedConfig
    {
        public Type EventType { get; }
        public Type ConsumerType { get; }
        public int MsDelay { get; }

        public EventAssignedConfig(Type eventType, Type consumerType, int msDelay)
        {
            EventType = eventType;
            ConsumerType = consumerType;
            MsDelay = msDelay;
        }
    }
}
