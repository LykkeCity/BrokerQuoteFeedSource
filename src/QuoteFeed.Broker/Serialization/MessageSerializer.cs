using System;
using System.Text;
using Lykke.RabbitMqBroker.Publisher;
using QuoteFeed.Core.Model;
using Newtonsoft.Json;

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
