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
        [Fact]
        public void NullIsIgnored()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            controller.ProcessOrderbook(null).Wait();
            Assert.Equal(0, publisher.Published.Count);
        }

        [Fact]
        public void EmptyAssetIsIgnored()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            controller.ProcessOrderbook(CreateOrder(null, true, Utils.ParseUtc("2017-01-01 10:10:12Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 100 }
                 })).Wait();

            controller.ProcessOrderbook(CreateOrder("", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 100 }
                 })).Wait();
            Assert.Equal(0, publisher.Published.Count);
        }

        [Fact]
        public void EmptyPricesIsIgnored()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            controller.ProcessOrderbook(CreateOrder("btc", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), null)).Wait();
            controller.ProcessOrderbook(CreateOrder("btc", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), new VolumePrice[] { })).Wait();
            Assert.Equal(0, publisher.Published.Count);
        }

        [Fact]
        public void InvalidDateIsIgnored()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            DateTime unspecified = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime local = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Local);

            controller.ProcessOrderbook(CreateOrder("btc", true, unspecified, new VolumePrice[] { new VolumePrice() { Volume = 1, Price = 100 } })).Wait();
            controller.ProcessOrderbook(CreateOrder("btc", true, local, new VolumePrice[] { new VolumePrice() { Volume = 1, Price = 100 } })).Wait();
            controller.ProcessOrderbook(CreateOrder("btc", true, DateTime.MinValue, new VolumePrice[] { new VolumePrice() { Volume = 1, Price = 100 } })).Wait();
            controller.ProcessOrderbook(CreateOrder("btc", true, DateTime.MaxValue, new VolumePrice[] { new VolumePrice() { Volume = 1, Price = 100 } })).Wait();
            Assert.Equal(0, publisher.Published.Count);
        }
    }
}
