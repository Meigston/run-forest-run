namespace TestRabbitMq.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Brokers.Common;
    using Newtonsoft.Json;
    using TestRabbitMq.Events;

    public class WeatherForecastConsumer : BaseMessageConsumer<WeatherForecastEvent>
    {
        public override async Task<bool> Consume(WeatherForecastEvent @event)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"{Environment.NewLine} Evento: {JsonConvert.SerializeObject(@event)}");
            });

            return true;
        }
    }
}
