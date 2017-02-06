using System;
using System.Text;
using Newtonsoft.Json;
using Lykke.RabbitMqBroker.Subscriber;
using QuoteFeed.Core.Model;

namespace QuoteFeed.Broker.Serialization
{
    public class MessageDeserializer : IMessageDeserializer<Order>
    {
        public Order Deserialize(byte[] data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                 DateTimeZoneHandling = DateTimeZoneHandling.Utc // treat datetime as Utc
            };

            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<Order>(json, settings);
        }
    }
}
