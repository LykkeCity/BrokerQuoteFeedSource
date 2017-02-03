using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using QuoteFeed.Core.Tests.Stub;
using QuoteFeed.Core.Model;

namespace QuoteFeed.Core.Tests
{
    public partial class QuoteFeedControllerTests
    {
        private const int TASKS_COUNT = 1000;

        [Fact]
        public void MultipleOrdersShouldBeProcessedConsecutively()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub(async (Quote q) => {
                await Task.Delay(TimeSpan.FromSeconds(1));
            });
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            List<Task> tasks = new List<Task>(TASKS_COUNT);
            for (int i = 0; i < TASKS_COUNT; i++)
            {
                var task = controller.ProcessOrderbook(new Order("btc", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), 
                    new[] { new VolumePrice() { Volume = 1, Price = i } }));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            Assert.Equal(TASKS_COUNT, publisher.Published.Count);
        }
    }
}
