namespace Brokers.Common.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRawEventPublisher
    {
        Task SendRaw(
            string eventBody,
            string routingKey,
            string exchange,
            string messageId = null,
            string correlationId = null,
            Dictionary<string, object> additionalHeaders = null);
    }
}
