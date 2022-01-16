namespace RunForestRun.RabbitMq.Events.Example
{
    using Brokers.Common;

    public class Test1Event : BaseEvent
    {
        public string Color { get; set; }

        public string InvertedColor { get; set; }
    }
}
