namespace TestRabbitMq.Events
{
    using System;
    using Brokers.Common;

    public class WeatherForecastEvent : BaseEvent
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string Summary { get; set; }
    }
}
