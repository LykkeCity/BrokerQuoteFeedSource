using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

using Common.HttpRemoteRequests;
using Common.Log;
using Lykke.Logs;
using Lykke.RabbitMqBroker;
using Lykke.SlackNotification.AzureQueue;
using AzureStorage.Tables;

namespace QuoteFeed.Broker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .Build();

            string settingsUrl = config.GetValue<string>("settingsUrl");

            // Reading app settings from settings web-site
            HttpRequestClient webClient = new HttpRequestClient();
            string json = webClient.GetRequest(settingsUrl, "application/json").Result;
            var appSettings = JsonConvert.DeserializeObject<AppSettings>(json);

            // Initialize slack sender
            var log = new LogToConsole();
            var slackSender = serviceCollection.UseSlackNotificationsSenderViaAzureQueue(appSettings.SlackNotifications.AzureQueue, log);

            // Initialize azure logger
            var logStorage = new LykkeLogToAzureStorage("FeedCandlesHistoryWriterBroker",
                new AzureTableStorage<LogEntity>(appSettings.QuotesCandlesHistory.LogsConnectionString, "FeedCandlesHistoryWriterBrokerLogs", log),
                slackSender);

            var mq = appSettings.RabbitMq;
            var rabitMqSubscriberSettings = new RabbitMqSettings()
            {
                ConnectionString = $"amqp://{mq.Username}:{mq.Password}@{mq.Host}:{mq.Port}",
                QueueName = mq.ExchangeOrderbook
            };

            var rabbitMqPublisherSettings = new RabbitMqSettings
            {
                ConnectionString = $"amqp://{mq.Username}:{mq.Password}@{mq.Host}:{mq.Port}",
                QueueName = mq.QuoteFeed
            };

            Broker broker = new Broker(rabitMqSubscriberSettings, rabbitMqPublisherSettings, logStorage);
            broker.Start();

            Console.WriteLine("Press any key...");
            Console.ReadLine();

            broker.Stop();
        }
    }
}
