using System;
using System.Text;
using Newtonsoft.Json;
using Lykke.Domain.Prices.Model;
using Lykke.RabbitMqBroker.Subscriber;

namespace QuoteFeed.Broker.Serialization
{
    public class MessageDeserializer : IMessageDeserializer<OrderBook>
    {
        public OrderBook Deserialize(byte[] data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                 DateTimeZoneHandling = DateTimeZoneHandling.Utc // treat datetime as Utc
            };

            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<OrderBook>(json, settings);
        }
    }
}
