namespace TestRabbitMq.Consumers
{
    using System.Threading.Tasks;
    using Brokers.Common;
    using TestRabbitMq.Events;

    public class Test1Consumer : BaseMessageConsumer<Test1Event>
    {
        public override Task<bool> Consume(Test1Event @event)
        {
            throw new System.NotImplementedException();
        }
    }
}
