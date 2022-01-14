using TestRabbitMq.Events;

namespace TestRabbitMq.Consumers
{
    using System.Threading.Tasks;

    using Brokers.Common;

    public class Test2Consumer : BaseMessageConsumer<Test2Event>
    {
        public override Task<bool> Consume(Test2Event @event)
        {
            throw new System.NotImplementedException();
        }
    }
}
