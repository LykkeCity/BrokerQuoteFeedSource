using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;

using AzureStorage.Tables;
using Common.Log;
using Lykke.Domain.Prices.Model;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Common;
using Common.Abstractions;

namespace QuoteFeed.Broker
{
    internal class Startup
    {
        private AppSettings settings;

        public static string ApplicationName { get { return "BrokerQuoteFeed"; } }

        public Startup(string settingsJson, ILog log)
        {
            this.settings = JsonConvert.DeserializeObject<AppSettings>(settingsJson);
        }

        public void ConfigureServices(ContainerBuilder builder, ILog log)
        {
            var mq = settings.BrokerQuoteFeed.RabbitMq;
            var connectionsString = $"amqp://{mq.Username}:{mq.Password}@{mq.Host}:{mq.Port}";
            var subscriberSettings = new RabbitMqSettings()
            {
                ConnectionString = connectionsString,
                QueueName = mq.ExchangeOrderbook
            };
            var publisherSettings = new RabbitMqSettings
            {
                ConnectionString = connectionsString,
                QueueName = mq.QuoteFeed
            };

            var subscriber = new RabbitMqSubscriber<Order>(subscriberSettings);
            var publisher = new RabbitMqPublisher<Quote>(publisherSettings);
            var broker = new Broker(subscriber, publisher, log);

            builder.RegisterInstance(subscriber)
                .As<IStartable>()
                .As<IStopable>();

            builder.RegisterInstance(publisher)
                .As<IStartable>()
                .As<IStopable>();
        }

        public void Configure(ILifetimeScope scope)
        {
        }
    }
}
