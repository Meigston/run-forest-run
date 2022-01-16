namespace RunForestRun.RabbitMq.Consumers.Example
{
    using System.Threading.Tasks;

    using Brokers.Common;

    using RunForestRun.RabbitMq.Events.Example;

    public class Test1Consumer : BaseMessageConsumer<Test1Event>
    {
        public override Task<bool> Consume(Test1Event @event)
        {
            throw new System.NotImplementedException();
        }
    }
}
