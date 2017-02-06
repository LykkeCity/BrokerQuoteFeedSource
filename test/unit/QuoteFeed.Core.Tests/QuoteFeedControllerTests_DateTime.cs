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
        /// <summary>
        /// Fact: Generated quotes have timestamps with Kind=Utc.
        /// </summary>
        [Fact]
        public void GeneratedQuotesHaveUtcKind()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(new Order("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(new Order("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1001 }, new VolumePrice() { Volume = 1, Price = 999 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), 1001.0));

            /// Check Kind
            foreach(var quote in publisher.Published)
            {
                Assert.Equal(0, quote.Timestamp.Kind.CompareTo(DateTimeKind.Utc));
            }
        }
    }
}
