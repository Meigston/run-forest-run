namespace RunForestRun.RabbitMq.Consumers.Example
{
    using System.Threading.Tasks;

    using Brokers.Common;

    using RunForestRun.RabbitMq.Events.Example;

    public class Test2Consumer : BaseMessageConsumer<Test2Event>
    {
        public override Task<bool> Consume(Test2Event @event)
        {
            throw new System.NotImplementedException();
        }
    }
}
