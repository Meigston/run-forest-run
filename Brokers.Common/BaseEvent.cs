namespace Brokers.Common
{
    using System;

    public abstract class BaseEvent
    {
        protected BaseEvent()
        {
            EventConfig = new EventConfig()
            {
                Date = DateTime.Now,
                Token = Guid.NewGuid().ToString(),
                EventId = Guid.NewGuid().ToString(),
                EventType = GetType().FullName
            };
        }

        public EventConfig EventConfig { get; }
    }
}
