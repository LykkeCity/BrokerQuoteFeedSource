using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Xunit;

using QuoteFeed.Broker.Serialization;
using QuoteFeed.Core.Model;

namespace QuoteFeed.Broker.Tests
{
    public class MessageSerializerTests
    {
        [Fact]
        public void SerializedDateIsUtc()
        {
            Quote model = new Quote()
            {
                AssetPair = "btcusd",
                IsBuy = true,
                Price = 1000,
                Timestamp = new DateTime(2017, 1, 31, 11, 38, 49, DateTimeKind.Utc)
            };

            var serializer = new MessageSerializer();
            byte[] bytes = serializer.Serialize(model);

            string json = Encoding.UTF8.GetString(bytes);

            Assert.Contains("2017-01-31T11:38:49Z", json);
        }
    }
}
