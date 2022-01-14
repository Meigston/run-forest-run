namespace Brokers.Common.Interfaces
{
    using System.Threading.Tasks;

    public interface IMessageConsumer
    {
        Task<bool> Consume<T>(T @event) where T : BaseEvent;
    }
}
