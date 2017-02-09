using System;
using System.Text;
using Newtonsoft.Json;
using Lykke.Domain.Prices.Model;
using Lykke.RabbitMqBroker.Publisher;

namespace QuoteFeed.Broker.Serialization
{
    public class MessageSerializer : IRabbitMqSerializer<Quote>
    {
        public byte[] Serialize(Quote model)
        {
            string json = JsonConvert.SerializeObject(model);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
