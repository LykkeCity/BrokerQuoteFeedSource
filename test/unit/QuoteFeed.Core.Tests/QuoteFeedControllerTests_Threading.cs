using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using QuoteFeed.Core.Tests.Stub;
using Lykke.Domain.Prices.Model;

namespace QuoteFeed.Core.Tests
{
    public partial class QuoteFeedControllerTests
    {
        private const int TASKS_COUNT = 1000;

        [Fact]
        public void MultipleOrdersShouldBeProcessedConsecutively()
        {
            LoggerStub logger = new LoggerStub();
            // Assume that publisher execution takes some time (random number of seconds)
            QuotePublisherStub publisher = new QuotePublisherStub(async (Quote q) => {
                int duration = new Random().Next(1, 3);
                await Task.Delay(TimeSpan.FromSeconds(duration));
            });
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // Produce multiple orders and wait for publishing
            List<Task> tasks = new List<Task>(TASKS_COUNT);
            for (int i = 0; i < TASKS_COUNT; i++)
            {
                var task = controller.ProcessOrderbook(CreateOrder("btc", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), 
                    new[] { new VolumePrice() { Volume = 1, Price = i } }));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            
            // There should be the same number of events as the number of incoming orders.
            // If orders were processed in different order, there would be less events
            // as the price is constantly increasing.
            Assert.Equal(TASKS_COUNT, publisher.Published.Count);
        }
    }
}
