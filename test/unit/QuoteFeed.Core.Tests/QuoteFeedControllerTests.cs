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
        /// Fact: Quote is updated on every new order, because all conditions are met.
        /// </summary>
        [Fact]
        public void QuoteIsUpdated()
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

            // BTCRUB / BUY, 1st order
            controller.ProcessOrderbook(new Order("btcrub", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 999 }
                 })).Wait();

            Assert.Equal(3, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcrub", true, Utils.ParseUtc("2017-01-01 10:10:11Z"), 999.0));

            // BTCEUR / BUY, 1st order
            controller.ProcessOrderbook(new Order("btceur", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 100 }, new VolumePrice() { Volume = 1, Price = 99 }
                 })).Wait();

            Assert.Equal(4, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btceur", true, Utils.ParseUtc("2017-01-01 10:10:12Z"), 100));

            // BTCUSD / SELL, 1st order
            controller.ProcessOrderbook(new Order("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(5, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCUSD / SELL, 2nd order
            controller.ProcessOrderbook(new Order("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 40 }, new VolumePrice() { Volume = 1, Price = 30 }
                 })).Wait();

            Assert.Equal(6, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcusd", false, Utils.ParseUtc("2017-01-02 10:10:11Z"), 30));
        }

        [Fact]
        public void QuoteIsNotUpdatedWhenPriceIsNotExtrem()
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
                    new VolumePrice() { Volume = 1, Price = 999 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(new Order("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(new Order("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
        }

        [Fact]
        public void QuoteIsNotUpdatedWhenDateIsOld()
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
            controller.ProcessOrderbook(new Order("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:09Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1001 }
                 })).Wait();

            Assert.Equal(1, publisher.Published.Count);

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(new Order("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(new Order("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:09Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 40 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
        }

        [Fact]
        public void QuoteIsUpdatedWhenDateIsEqual()
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
            controller.ProcessOrderbook(new Order("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 1001 }
                 })).Wait();

            Assert.Equal(2, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcusd", true, Utils.ParseUtc("2017-01-01 10:10:10Z"), 1001.0));

            // BTCRUB / SELL, 1st order
            controller.ProcessOrderbook(new Order("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 50 }, new VolumePrice() { Volume = 1, Price = 60 }
                 })).Wait();

            Assert.Equal(3, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 50));

            // BTCRUB / SELL, 2nd order
            controller.ProcessOrderbook(new Order("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), new[] {
                     new VolumePrice() { Volume = 1, Price = 40 }
                 })).Wait();

            Assert.Equal(4, publisher.Published.Count);
            publisher.Published.Last().Equals(new Quote("btcrub", false, Utils.ParseUtc("2017-01-02 10:10:10Z"), 40));
        }
    }
}
