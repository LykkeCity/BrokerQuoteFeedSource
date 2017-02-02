using System;
using System.Collections.Generic;

namespace QuoteFeed.Core.Model
{
    public class Order
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<VolumePrice> Prices { get; set; } = new List<VolumePrice>();
    }
}
