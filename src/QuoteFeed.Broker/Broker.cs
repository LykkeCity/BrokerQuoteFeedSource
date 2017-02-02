using System;
using System.Threading.Tasks;

using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.RabbitMqBroker.Publisher;

using QuoteFeed.Broker.Serialization;
using QuoteFeed.Core.Model;
using QuoteFeed.Core;
using QuoteFeed.Core.Contracts;

namespace QuoteFeed.Broker
{
    public interface IStartable
    {
        void Start();
    }

    public interface IStoppable
    {
        void Stop();
    }

    internal sealed class Broker : IQuotePublisher, IStartable, IStoppable, IDisposable
    {
        private RabbitMqSubscriber<Order> subscriber;
        private RabbitMqPublisher<Quote> publisher;
        private QuoteFeedController controller;
        private ILog logger;

        private bool isStarted = false;
        private bool isDisposed = false;

        public Broker(
            RabbitMqSettings rabitMqSubscriberSettings, 
            RabbitMqSettings rabbitMqPublisherSettings, 
            ILog logger)
        {
            this.subscriber =
                new RabbitMqSubscriber<Order>(rabitMqSubscriberSettings)
                  .SetMessageDeserializer(new MessageDeserializer())
                  .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                  .Subscribe(HandleMessage)
                  .SetLogger(logger);

            this.publisher =
                new RabbitMqPublisher<Quote>(rabbitMqPublisherSettings)
                .SetLogger(logger)
                .SetSerializer(new MessageSerializer());

            this.logger = logger;

            controller = new QuoteFeedController(this, logger);
        }

        public async Task Publish(Quote quote)
        {
            await this.publisher.ProduceAsync(quote);
        }

        public void Start()
        {
            EnsureNotDisposed();
            if (!this.isStarted)
            {
                this.subscriber.Start();
                this.publisher.Start();
                this.isStarted = true;
            }
        }

        public void Stop()
        {
            EnsureNotDisposed();
            if (this.isStarted)
            {
                this.subscriber.Stop();
                this.publisher.Stop();
                this.isStarted = false;
            }
        }

        private async Task HandleMessage(Order order)
        {
            await this.controller.ProcessOrderbook(order);
        }

        #region "IDisposable implementation"
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Broker()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // get rid of managed resources
            }
            // get rid of unmanaged resources
            this.isDisposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                // TODO: Fill missing arguments
                this.logger.WriteErrorAsync("", "", "", new InvalidOperationException("Disposed object Broker has been called"), DateTime.Now);
            }
        }
        #endregion
    }
}
