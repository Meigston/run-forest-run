namespace Brokers.Common.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IEventPublisher : IDisposable
    {
        Task Send<TEvent>(TEvent @event) where TEvent : BaseEvent;

        Task Send<TEvent, TOldEvent>(TEvent @event, TOldEvent oldEvent) where TEvent : BaseEvent where TOldEvent : BaseEvent;
    }
}
