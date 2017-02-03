using System;
using System.Collections.Generic;

namespace QuoteFeed.Core.Model
{
    public class Order
    {
        public Order()
        {
            this.Prices = new List<VolumePrice>();
        }

        public Order(string assetPair, bool isBuy, DateTime timestamp, IEnumerable<VolumePrice> prices)
        {
            this.AssetPair = assetPair;
            this.IsBuy = isBuy;
            this.Timestamp = timestamp;
            this.Prices = (prices != null) ? new List<VolumePrice>(prices) : new List<VolumePrice>();
        }

        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<VolumePrice> Prices { get; set; }
    }
}
