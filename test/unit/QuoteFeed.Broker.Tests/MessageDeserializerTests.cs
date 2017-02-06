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
    public class MessageDeserializerTests
    {
        [Fact]
        public void UnspecifiedDateDeserializedToUtc()
        {
            // Kind is unspecified
            string json = "{\"assetPair\":\"BTCUSD\",\"isBuy\":true,\"timestamp\":\"Jan 31, 2017 11:38:49 AM\","
                + "\"prices\":[{\"volume\":0.52,\"price\":933.89},{\"volume\":43.78618975,\"price\":933.88},{\"volume\":6.4028,\"price\":933.62}]}";

            var deserializer = new MessageDeserializer();
            Order model = deserializer.Deserialize(Encoding.UTF8.GetBytes(json));

            Assert.Equal(model.Timestamp, new DateTime(2017, 1, 31, 11, 38, 49, DateTimeKind.Utc));
            Assert.Equal(0, model.Timestamp.Kind.CompareTo(DateTimeKind.Utc));
        }

        [Fact]
        public void UtcDateDeserializedToUtc()
        {
            // Kind is set to Utc
            string json = "{\"assetPair\":\"BTCUSD\",\"isBuy\":true,\"timestamp\":\"2009-02-15T15:02:00Z\","
                + "\"prices\":[{\"volume\":0.52,\"price\":933.89},{\"volume\":43.78618975,\"price\":933.88},{\"volume\":6.4028,\"price\":933.62}]}";

            var deserializer = new MessageDeserializer();
            Order model = deserializer.Deserialize(Encoding.UTF8.GetBytes(json));

            Assert.Equal(model.Timestamp, new DateTime(2009, 2, 15, 15, 2, 0, DateTimeKind.Utc));
            Assert.Equal(0, model.Timestamp.Kind.CompareTo(DateTimeKind.Utc));
        }
    }
}
