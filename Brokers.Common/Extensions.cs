namespace Brokers.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Brokers.Common.Interfaces;

    using Microsoft.Extensions.DependencyInjection;

    public static class Extensions
    {
        public static readonly Dictionary<string, EventAssignedConfig> AssignedEvents;

        static Extensions()
        {
            AssignedEvents = new Dictionary<string, EventAssignedConfig>();
        }

        public static void SubscribeEvent<TEvent, TConsumer>(this IServiceCollection serviceCollection, int msDelay = 0)
            where TEvent : BaseEvent
            where TConsumer : class, IMessageConsumer
        {
            serviceCollection.AddScoped<IMessageConsumer, TConsumer>();
            AssignedEvents.Add(typeof(TEvent).FullName, new EventAssignedConfig(typeof(TEvent), typeof(TConsumer), msDelay));
        }

        public static void SubscribeTypedEvent<TEvent, TConsumer>(this IServiceCollection serviceCollection, int msDelay = 0)
            where TEvent : BaseEvent
            where TConsumer : BaseMessageConsumer<TEvent>, IMessageConsumer
        {
            serviceCollection.AddScoped<IMessageConsumer, TConsumer>();
            AssignedEvents.Add(typeof(TEvent).FullName, new EventAssignedConfig(typeof(TEvent), typeof(TConsumer), msDelay));
        }

        public static IMessageConsumer GetConsumerService(this IServiceProvider serviceProvider, Type eventType)
        {
            var service = serviceProvider.GetServices<IMessageConsumer>()
                .FirstOrDefault(a => a.GetType() == AssignedEvents[eventType.FullName].ConsumerType);

            return service;
        }
    }
}
