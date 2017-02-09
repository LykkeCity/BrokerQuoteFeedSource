using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using Common.Log;
using QuoteFeed.Core.Contracts;
using QuoteFeed.Core.Model;

namespace QuoteFeed.Core
{
    public sealed class QuoteFeedController
    {
        private readonly static string PROCESS = "QuoteFeedController";

        private readonly IQuotePublisher publisher;
        private readonly ILog logger;
        private readonly string componentName;
        // Storage for current quotes. Samples:
        // { "BTCUSD_buy", { asset: "BTCUSD", isBuy: true, price: 100, timestamp: ... } }
        // { "BTCUSD_sell", { asset: "BTCEUR", isBuy: false, price: 101, timestamp: ... } }
        private readonly Dictionary<string, Quote> currentQuotes = new Dictionary<string, Quote>();

        public QuoteFeedController(IQuotePublisher publisher, ILog logger, string componentName = "")
        {
            this.publisher = publisher;
            this.logger = logger;
            this.componentName = componentName;
        }

        public async Task ProcessOrderbook(Order order)
        {
            // Validate
            // 
            ICollection<string> validationErrors = this.Validate(order);
            if (validationErrors.Count > 0)
            {
                foreach(string error in validationErrors)
                {
                    await this.logger.WriteErrorAsync(this.componentName, PROCESS, String.Empty, new ArgumentException("Received invalid order. " + error));
                }
                return; // Skipping invalid orders
            }

            if (order.Prices.Count == 0)
            {
                await this.logger.WriteWarningAsync(this.componentName, PROCESS, String.Empty, "Order does not contain any prices. Skipping.");
                return; // Skipping empty orders
            }

            // Calculate order min/max
            double extremPrice = order.Prices.Select(vp => vp.Price).Aggregate((extPrice, curPrice) => {
                return (order.IsBuy) ? Math.Max(extPrice, curPrice) : Math.Min(extPrice, curPrice);
            });

            // Compare new order with current min/max
            Quote currentQuote;
            bool isUpdated = false;
            string key = order.AssetPair + (order.IsBuy ? "_buy" : "_sell");
            if (!currentQuotes.TryGetValue(key, out currentQuote))
            {
                // Initialize quote
                currentQuote = new Quote()
                {
                    AssetPair = order.AssetPair,
                    IsBuy = order.IsBuy,
                    Price = extremPrice,
                    Timestamp = order.Timestamp
                };
                isUpdated = true;
            }
            else if ((order.Timestamp > currentQuote.Timestamp && extremPrice != currentQuote.Price)
                || (order.Timestamp == currentQuote.Timestamp 
                    && ((order.IsBuy && extremPrice > currentQuote.Price) || (!order.IsBuy && extremPrice < currentQuote.Price)))
                )
            {
                // Update quote when price changed and datetime is newer or when datetime is the same but price is better
                currentQuote = new Quote()
                {
                    AssetPair = order.AssetPair,
                    IsBuy = order.IsBuy,
                    Price = extremPrice,
                    Timestamp = order.Timestamp
                };
                isUpdated = true;
            }

            if (isUpdated)
            {
                // Save new value
                currentQuotes[key] = currentQuote;
                // Publish update
                await this.publisher.Publish(currentQuote);
            }
        }

        private ICollection<string> Validate(Order order)
        {
            List<string> errors = new List<string>();

            if (order == null)
            {
                errors.Add("Argument 'Order' is null.");
            }
            if (order != null && string.IsNullOrEmpty(order.AssetPair))
            {
                errors.Add(string.Format("Invalid 'AssetPair': '{0}'", order.AssetPair ?? ""));
            }
            if (order != null && (order.Timestamp == DateTime.MinValue || order.Timestamp == DateTime.MaxValue))
            {
                errors.Add(string.Format("Invalid 'Timestamp' range: '{0}'", order.Timestamp));
            }
            if (order != null && order.Timestamp.Kind != DateTimeKind.Utc)
            {
                errors.Add(string.Format("Invalid 'Timestamp' Kind (UTC is required): '{0}'", order.Timestamp));
            }
            if (order != null && order.Prices == null)
            {
                errors.Add("Invalid 'Price': null");
            }

            return errors;
        }
    }
}
