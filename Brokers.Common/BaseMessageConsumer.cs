namespace Brokers.Common
{
    using System;
    using System.Threading.Tasks;

    using Brokers.Common.Interfaces;

    public abstract class BaseMessageConsumer<T> : IMessageConsumer where T : BaseEvent
    {
        public abstract Task<bool> Consume(T @event);

        Task<bool> IMessageConsumer.Consume<T1>(T1 @event)
        {
            if (!(@event is T))
            {
                throw new ArgumentException("Tipo do evento não identificado", nameof(@event));
            }

            return Consume(@event as T);
        }
    }
}
