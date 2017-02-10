using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Lykke.Domain.Prices.Model;
using QuoteFeed.Core.Tests.Stub;

namespace QuoteFeed.Core.Tests
{
    public partial class QuoteFeedControllerTests
    {
        /// <summary>
        /// Fact: Quote is updated on every new order, because all conditions are met.
        /// </summary>
        [Fact]
        public void QuoteIsUpdated()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1001 }, new VolumePrice() { Volume = 1, Price = 999 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), 1001.0));

            // BTCRUB / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcrub", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 999 }
                 })).Wait();

            Assert.Equal(3, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), 999.0));

            // BTCEUR / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btceur", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 100 }, new VolumePrice() { Volume = 1, Price = 99 }
                 })).Wait();

            Assert.Equal(4, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btceur", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), 100));

            // BTCUSD / SELL, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(5, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCUSD / SELL, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 40 }, new VolumePrice() { Volume = 1, Price = 30 }
                 })).Wait();

            Assert.Equal(6, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), 30));
        }

        [Fact]
        public void QuoteIsNotUpdatedWhenPriceIsSame()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
        }

        [Fact]
        public void QuoteIsUpdatedOnPriceChanges()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 1.0 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), 1.0));

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(3, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 55 }
                 })).Wait();

            Assert.Equal(4, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), 55));
        }

        [Fact]
        public void QuoteIsNotUpdatedWhenDateIsOld()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:09Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1001 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:09Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 40 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
        }

        [Fact]
        public void QuoteIsUpdatedWhenDateIsEqualAndBestPrice()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1001 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1001.0));

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(3, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 40 }
                 })).Wait();

            Assert.Equal(4, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 40));
        }

        [Fact]
        public void QuoteIsNotUpdatedWhenDateIsEqualAndNotBestPrice()
        {
            LoggerStub logger = new LoggerStub();
            QuotePublisherStub publisher = new QuotePublisherStub();
            QuoteFeedController controller = new QuoteFeedController(publisher, logger);

            // BTCUSD / BUY, 1st order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                    new VolumePrice() { Volume = 1, Price = 999 }, new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1000.0));

            // BTCUSD / BUY, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1000 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);

            // BTCUSD / BUY, 3nd order
            controller.ProcessOrderbook(CreateOrder("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 900 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(CreateQuote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);

            // BTCRUB / SELL, 3nd order
            controller.ProcessOrderbook(CreateOrder("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 70 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
        }

        private Order CreateOrder(string asset, bool isBuy, DateTime timestamp, VolumePrice[] prices)
        {
            return new Order()
            {
                AssetPair = asset,
                IsBuy = isBuy,
                Timestamp = timestamp,
                Prices = prices
            };
        }

        private Quote CreateQuote(string asset, bool isBuy, DateTime timestamp, double price)
        {
            return new Quote()
            {
                AssetPair = asset,
                IsBuy = isBuy,
                Timestamp = timestamp,
                Price = price
            };
        }
    }
}
