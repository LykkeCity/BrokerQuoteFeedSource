using System;
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
        private static readonly string COMPONENT = "BrokerQuoteFeed";

        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            var consoleLog = new LogToConsole();
            var loggerFanout = new LoggerFanout()
                .AddLogger("console", consoleLog);

            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Loading \"BrokerQuoteFeed\".").Wait();
            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Reading app settings.").Wait();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .Build();

            string settingsUrl = config.GetValue<string>("settingsUrl");

            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Loading app settings from web-site.").Wait();
            // Reading app settings from settings web-site
            HttpRequestClient webClient = new HttpRequestClient();
            string json = webClient.GetRequest(settingsUrl, "application/json").Result;
            var appSettings = JsonConvert.DeserializeObject<AppSettings>(json);

            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Initializing azure/slack logger.").Wait();
            // Initialize slack sender
            var slackSender = serviceCollection.UseSlackNotificationsSenderViaAzureQueue(appSettings.SlackNotifications.AzureQueue, consoleLog);
            // Initialize azure logger
            var azureLog = new LykkeLogToAzureStorage("FeedCandlesHistoryWriterBroker",
                new AzureTableStorage<LogEntity>(appSettings.QuotesCandlesHistory.LogsConnectionString, "FeedCandlesHistoryWriterBrokerLogs", consoleLog),
                slackSender);
            loggerFanout.AddLogger("azure", azureLog);

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

            // Start broker
            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Starting queue subscription.").Wait();
            Broker broker = new Broker(rabitMqSubscriberSettings, rabbitMqPublisherSettings, loggerFanout);
            broker.Start();

            Console.WriteLine("Press any key...");
            Console.ReadLine();

            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Stopping broker.").Wait();
            broker.Stop();
            loggerFanout.WriteInfoAsync(COMPONENT, string.Empty, string.Empty, "Brokker is stopped.").Wait();
        }
    }
}
