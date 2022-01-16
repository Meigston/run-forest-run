namespace RunForestRun.RabbitMq.Consumers.Example
{
    using Brokers.Common;
    using Brokers.RabbitMq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using RunForestRun.RabbitMq.Events.Example;

    public static class Startup
    {
        public static IHostBuilder UseStartup(this IHostBuilder builder)
        {
            builder.ConfigureServices((hctx, services) =>
                {
                    ConfigureServices(services, hctx.Configuration);
                });
            return builder;
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.UseRabbitMqSubscriber(a => new RabbitMqOptions("localhost", 5672, "user", "password", "TestRabbitMq.Consumers") { VirtualHost = "/" });

            services.SubscribeTypedEvent<WeatherForecastEvent, WeatherForecastConsumer>();
            services.SubscribeTypedEvent<Test2Event, Test2Consumer>();
            services.SubscribeTypedEvent<Test1Event, Test1Consumer>();
        }
    }
}
