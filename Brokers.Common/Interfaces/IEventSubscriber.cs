namespace Brokers.Common.Interfaces
{
    using System;

    public interface IEventSubscriber : IDisposable
    {
        void StartListener();
    }
}
