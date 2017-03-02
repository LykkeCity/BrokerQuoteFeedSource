using System;
using System.Threading.Tasks;

using Common;
using Common.Log;
using Lykke.Domain.Prices.Model;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.RabbitMqBroker.Publisher;

using QuoteFeed.Broker.Serialization;
using QuoteFeed.Core;
using QuoteFeed.Core.Contracts;

namespace QuoteFeed.Broker
{
    internal sealed class Broker : IQuotePublisher
    {
        private readonly static string COMPONENT_NAME = "BrokerQuoteFeed";
        private readonly static string PROCESS = "Broker";

        private RabbitMqSubscriber<OrderBook> subscriber;
        private RabbitMqPublisher<Quote> publisher;
        private QuoteFeedController controller;
        private ILog logger;

        private bool isStarted = false;
        private bool isDisposed = false;

        public Broker(
            RabbitMqSubscriber<OrderBook> subscriber,
            RabbitMqPublisher<Quote> publisher, 
            ILog logger)
        {
            this.logger = logger;
            this.subscriber = subscriber;
            this.publisher = publisher;

            subscriber
                  .SetMessageDeserializer(new MessageDeserializer())
                  .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                  .Subscribe(HandleMessage)
                  .SetLogger(logger);

            publisher
                .SetPublishStrategy(new DefaultFnoutPublishStrategy("", durable: true))
                .SetSerializer(new MessageSerializer())
                .SetLogger(logger);

            this.controller = new QuoteFeedController(this, logger, COMPONENT_NAME);
        }

        public async Task Publish(Quote quote)
        {
            if (quote != null)
            {
                await this.publisher.ProduceAsync(quote);
            }
            else
            {
                await this.logger.WriteErrorAsync(COMPONENT_NAME, string.Empty, string.Empty, 
                    new ArgumentNullException(nameof(quote), "Tried to publish <NULL> message."));
            }
        }


        private async Task HandleMessage(OrderBook order)
        {
            if (order != null)
            {
                await this.controller.ProcessOrderbook(order);
            }
            else
            {
                await this.logger.WriteWarningAsync(COMPONENT_NAME, string.Empty, string.Empty, "Received order <NULL>.");
            }
        }
    }
}
