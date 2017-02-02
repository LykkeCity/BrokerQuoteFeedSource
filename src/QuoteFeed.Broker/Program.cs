using System;
using System.Text;
using System.Threading.Tasks;

using Common.Log;
using Lykke.RabbitMqBroker;

namespace QuoteFeed.Broker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rabitMqSubscriberSettings = new RabbitMqSettings()
            {
                ConnectionString = "",
                QueueName = ""
            };

            var rabbitMqPublisherSettings = new RabbitMqSettings
            {
                ConnectionString = "",
                QueueName = ""
            };
            // var factory = new ConnectionFactory { Uri = RabbitMqSettings.ConnectionString };

            ILog logger = null;

            Broker broker = new Broker(rabitMqSubscriberSettings, rabbitMqPublisherSettings, logger);
            broker.Start();
        }
    }
}
