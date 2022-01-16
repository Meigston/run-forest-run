namespace RunForestRun.RabbitMq.Events.Example
{
    using System;

    using Brokers.Common;

    public class Test2Event : BaseEvent
    {
        public string Name { get; set; }

        public DateTime BirthDay { get; set; }
    }
}
