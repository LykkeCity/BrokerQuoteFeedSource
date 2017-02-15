using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

using Common.Application;
using Common.HttpRemoteRequests;
using Common.Log;
using Lykke.Logs;
using Lykke.SlackNotification.AzureQueue;
using AzureStorage.Tables;

namespace QuoteFeed.Broker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Initialize logger
            var consoleLog = new LogToConsole();
            var logAggregate = new LogAggregate()
                .AddLogger(consoleLog);
            var log = logAggregate.CreateLogger();

            try
            {
                log.Info("Reading application settings.");
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var settingsUrl = config.GetValue<string>("BROKER_SETTINGS_URL");

                log.Info("Loading app settings from web-site.");
                string settingsJson = LoadSettings(settingsUrl);
                var appSettings = JsonConvert.DeserializeObject<AppSettings>(settingsJson);

                log.Info("Initializing azure/slack logger.");
                var services = new ServiceCollection(); // only used for azure logger
                logAggregate.ConfigureAzureLogger(services, Startup.ApplicationName, appSettings);

                log = logAggregate.CreateLogger();

                // After log is configured
                //
                log.Info("Creating Startup.");
                var startup = new Startup(settingsJson, log);

                log.Info("Configure startup services.");
                startup.ConfigureServices(Application.Instance.ContainerBuilder, log);

                log.Info("Starting application.");
                var scope = Application.Instance.Start();

                log.Info("Configure startup.");
                startup.Configure(scope);

                log.Info("Running application.");
                Application.Instance.Run();

                log.Info("Exit application.");
            }
            catch (Exception ex)
            {
                log.WriteErrorAsync("Program", string.Empty, string.Empty, ex).Wait();
            }
        }

        private static string LoadSettings(string url)
        {
            HttpRequestClient webClient = new HttpRequestClient();
            return webClient.GetRequest(url, "application/json").Result;
        }
    }

    internal static class LogExtensions
    {
        public static void ConfigureAzureLogger(this LogAggregate logAggregate, IServiceCollection services, string appName, AppSettings appSettings)
        {
            var log = logAggregate.CreateLogger();
            var slackSender = services.UseSlackNotificationsSenderViaAzureQueue(appSettings.SlackNotifications.AzureQueue, log);
            var azureLog = new LykkeLogToAzureStorage(appName,
                new AzureTableStorage<LogEntity>(appSettings.ApplicationLogs.AzureConnectionString, appName + "Logs", log),
                slackSender);
            logAggregate.AddLogger(azureLog);
        }

        public static void Info(this ILog log, string info)
        {
            log.WriteInfoAsync("BrokerQuoteFeed", "Program", string.Empty, info).Wait();
        }
    }
}
