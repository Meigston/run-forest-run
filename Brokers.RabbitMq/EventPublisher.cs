namespace Brokers.RabbitMq
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;

    using Brokers.Common;
    using Brokers.Common.Interfaces;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using RabbitMQ.Client;

    public class EventPublisher : IEventPublisher
    {
        internal IModel Channel { get; set; }

        private readonly ILogger _logger;

        public EventPublisher(IConnection connection,
            ILogger<EventPublisher> logger)
        {
            Channel = connection.CreateModel();
            _logger = logger;
        }

        public async Task Send<TEvent>(TEvent @event) where TEvent : BaseEvent
        {
            @event.EventConfig.Token = Aggregator.CurrentAggregationId;

            await PrivateSend(@event);
        }

        public async Task Send<TEvent, TOldEvent>(TEvent @event, TOldEvent oldEvent) where TEvent : BaseEvent where TOldEvent : BaseEvent
        {
            ConfigureEventsHistory(@event, oldEvent);

            await PrivateSend(@event);
        }

        public async Task SendDelayed<TEvent>(TEvent @event, TimeSpan delay) where TEvent : BaseEvent
        {
            @event.EventConfig.Token = Aggregator.CurrentAggregationId;

            await PrivateSendDelayed(@event, delay);
        }

        public async Task SendDelayed<TEvent, TOldEvent>(TEvent @event, TimeSpan delay, TOldEvent oldEvent) where TEvent : BaseEvent where TOldEvent : BaseEvent
        {
            ConfigureEventsHistory(@event, oldEvent);

            await PrivateSendDelayed(@event, delay);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Channel.IsOpen)
            {
                Channel.Close();
            }
        }

        private async Task PrivateSendDelayed<TEvent>(TEvent @event, TimeSpan delay) where TEvent : BaseEvent
        {
            var type = @event.GetType();

            var delayString = delay.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

            @event.EventConfig.Delayed = delayString;

            var exchangeName = Channel.ConfigureEventExchange<TEvent>();
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));


            await Task.Run(() =>
            {
                Channel.BasicPublish(exchange: exchangeName, routingKey: type.FullName, basicProperties: null, body: body);
            });
        }

        private async Task PrivateSend<TEvent>(TEvent @event) where TEvent : BaseEvent
        {
            var type = @event.GetType();
            var exchangeName = Channel.ConfigureEventExchange<TEvent>();
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            await Task.Run(() =>
            {
                Channel.BasicPublish(exchange: exchangeName, routingKey: type.FullName, basicProperties: null, body: body);
            });
        }

        private void ConfigureEventsHistory<TEvent, TOldEvent>(TEvent @event, TOldEvent oldEvent) where TEvent : BaseEvent where TOldEvent : BaseEvent
        {
            if (@event.EventConfig == null || oldEvent.EventConfig == null)
            {
                return;
            }

            var historicoIds = string.IsNullOrEmpty(oldEvent.EventConfig.HistoryIds) ?
                oldEvent.EventConfig.EventId :
                $"{oldEvent.EventConfig.HistoryIds} -> {oldEvent.EventConfig.EventId}";

            if (string.IsNullOrEmpty(@event.EventConfig.HistoryIds))
            {
                @event.EventConfig.HistoryIds = historicoIds;
            }
            else
            {
                @event.EventConfig.HistoryIds += $" -> {historicoIds}";
            }

            var historicoTypes = string.IsNullOrEmpty(oldEvent.EventConfig.HistoryTypes) ?
                oldEvent.EventConfig.EventType :
                $"{oldEvent.EventConfig.HistoryTypes} -> {oldEvent.EventConfig.EventType}";

            if (string.IsNullOrEmpty(@event.EventConfig.HistoryTypes))
            {
                @event.EventConfig.HistoryTypes = historicoTypes;
            }
            else
            {
                @event.EventConfig.HistoryTypes += $" -> {historicoTypes}";
            }

            @event.EventConfig.Token = oldEvent.EventConfig.Token;
        }
    }
}
