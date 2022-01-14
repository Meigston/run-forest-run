namespace Brokers.RabbitMq
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Brokers.Common;
    using Brokers.Common.Interfaces;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;

    using RabbitMQ.Client;

    public static class Extensions
    {
        private static bool _registeredFactory;

        /// <summary>
        /// Define the RabbitMqOptions in Subscriber
        /// </summary>
        /// <param name="serviceCollection">instance of IServiceCollection</param>
        /// <param name="implementationFactory">Your implementation of RabbitMqOptions</param>
        public static void UseRabbitMqSubscriber(this IServiceCollection serviceCollection, Func<IServiceProvider, RabbitMqOptions> implementationFactory)
        {
            UseDefaultConnFactory(serviceCollection, implementationFactory);
            DefineRabbitMqSubscriber(serviceCollection);
        }

        /// <summary>
        /// Use by default the options of setted in configuration with the section "RabbitMQConfig" in Subscriber
        /// </summary>
        /// <param name="serviceCollection">instance of IServiceCollection</param>
        public static void UseRabbitMqSubscriber(this IServiceCollection serviceCollection)
        {
            serviceCollection.UseDefaultConnFactory();
            DefineRabbitMqSubscriber(serviceCollection);
        }

        /// <summary>
        /// Define the RabbitMqOptions in Publisher
        /// </summary>
        /// <param name="serviceCollection">instance of IServiceCollection</param>
        /// <param name="implementationFactory">Your implementation of RabbitMqOptions</param>
        public static void UseRabbitMqPublisher(this IServiceCollection serviceCollection, Func<IServiceProvider, RabbitMqOptions> implementationFactory)
        {
            UseDefaultConnFactory(serviceCollection, implementationFactory);
            DefineRabbitMqPublisher(serviceCollection);
        }

        /// <summary>
        /// Define the RabbitMqOptions in publisher
        /// </summary>
        /// <param name="serviceCollection">instance of IServiceCollection</param>
        public static void UseRabbitMqPublisher(this IServiceCollection serviceCollection)
        {
            serviceCollection.UseDefaultConnFactory();
            DefineRabbitMqPublisher(serviceCollection);
        }

        private static void DefineRabbitMqPublisher(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IEventPublisher>(sp =>
                {
                    var connFactory = sp.GetService<ConnectionFactories>();
                    return new EventPublisher(connFactory.DefaultConnection, sp.GetService<ILogger<EventPublisher>>());
                });


            serviceCollection.AddTransient<IRawEventPublisher>(sp =>
                {
                    var connFactory = sp.GetService<ConnectionFactories>();
                    return new RawEventPublisher(connFactory.DefaultConnection, sp.GetService<ILogger<EventPublisher>>());
                });
        }

        private static void DefineRabbitMqSubscriber(IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ConnectionFactories>(a =>
                {
                    var opts = a.GetService<RabbitMqOptions>();
                    var connFactory = new ConnectionFactories()
                    {
                        AsyncFactory = GetConnFactory(opts, true),
                        Default = GetConnFactory(opts, true),
                        ClientName = opts.ClientName
                    };

                    connFactory.DefaultConnection = connFactory.Default.CreateConnection(connFactory.ClientName + "-Publisher");
                    connFactory.AsyncConnection = connFactory.AsyncFactory.CreateConnection(connFactory.ClientName + "-Consumer");

                    return connFactory;
                });

            serviceCollection.AddTransient<IEventSubscriber>(sp =>
                {
                    var connFactory = sp.GetService<ConnectionFactories>();
                    return new EventSubscriber(
                        connFactory.AsyncConnection,
                        sp.GetService<IServiceScopeFactory>(),
                        sp.GetService<ILogger<EventSubscriber>>(),
                        sp.GetService<RabbitMqOptions>());
                });
            serviceCollection.AddHostedService<EventListenerService>();
        }

        private static void UseDefaultConnFactory(this IServiceCollection serviceCollection, Func<IServiceProvider, RabbitMqOptions> implementationFactory = null)
        {
            if (_registeredFactory)
            {
                return;
            }

            if (implementationFactory == null)
            {
                serviceCollection.AddTransient<RabbitMqOptions>(
                sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var opts = new RabbitMqOptions();
                    configuration.Bind("RabbitMQConfig", opts);

                    return opts;
                });
            }
            else
            {
                serviceCollection.AddTransient<RabbitMqOptions>(implementationFactory);
            }

            serviceCollection.TryAddSingleton<ConnectionFactories>(a =>
            {
                var opts = a.GetService<RabbitMqOptions>();
                var connFactory = new ConnectionFactories()
                {
                    AsyncFactory = GetConnFactory(opts, true),
                    Default = GetConnFactory(opts, true),
                    ClientName = opts.ClientName
                };

                connFactory.DefaultConnection = connFactory.Default.CreateConnection(connFactory.ClientName + "-Publisher");
                connFactory.AsyncConnection = connFactory.AsyncFactory.CreateConnection(connFactory.ClientName + "-Consumer");

                return connFactory;
            });

            _registeredFactory = true;
        }

        private static IConnectionFactory GetConnFactory(RabbitMqOptions opts, bool asyncFact)
        {
            ConnectionFactory connFactory = new ConnectionFactory()
            {
                DispatchConsumersAsync = asyncFact
            };

            var uri = opts.Uri;
            if (uri != null)
            {
                connFactory.Uri = new Uri(uri);
            }
            else
            {
                connFactory.UserName = opts.UserName;
                connFactory.Password = opts.PassWord;
                connFactory.VirtualHost = opts.VirtualHost;
                connFactory.HostName = opts.HostName;
                if (opts.Port.HasValue)
                {
                    connFactory.Port = opts.Port.Value;
                }
            }

            return connFactory;
        }

        private static string ConfigureEventExchange(this IModel channel, Type type)
        {
            var configureExchange = GetExchangeName(type);
            channel.ExchangeDeclare(configureExchange, ExchangeType.Topic, true);
            return configureExchange;
        }

        private static string GetExchangeName(Type type)
        {
            return type.Assembly.GetName().Name + ".Exchange";
        }

        private static string GetQueueName(Assembly assembly)
        {
            return assembly.GetName().Name + ".Queue";
        }

        internal static string ConfigureEventExchange<TEventType>(this IModel channel)
        {
            Type type = typeof(TEventType);
            return ConfigureEventExchange(channel, type);
        }

        internal static string ConfigureListenerQueue(this IModel channel)
        {
            var queues = new List<string>();
            var consumerQueueName = GetQueueName(Assembly.GetEntryAssembly());
            queues.Add(consumerQueueName);
            foreach (var queue in queues)
            {
                channel.QueueDeclare(queue, true, false, false);

                foreach (var assignedEvent in Common.Extensions.AssignedEvents)
                {
                    var exchange = channel.ConfigureEventExchange(assignedEvent.Value.EventType);
                    channel.QueueBind(queue, exchange, assignedEvent.Key);
                }
            }

            return consumerQueueName;
        }
    }
}
