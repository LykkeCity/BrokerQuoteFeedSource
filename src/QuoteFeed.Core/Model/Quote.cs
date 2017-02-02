using System;

namespace QuoteFeed.Core.Model
{
    public class Quote
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
