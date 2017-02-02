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
        private readonly static string SERVICE_NAME = "QuoteFeed";

        private IQuotePublisher publisher;
        private ILog logger;
        private Dictionary<string, Quote> currentQuotes = new Dictionary<string, Quote>();

        public QuoteFeedController(IQuotePublisher publisher, ILog logger)
        {
            this.publisher = publisher;
            this.logger = logger;
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
                    // TODO: Fill missing values
                    await this.logger.WriteErrorAsync(SERVICE_NAME, "", "", new ArgumentException("Received invalid order. " + error));
                }
                return; // Skipping invalid orders
            }

            if (order.Prices.Count == 0)
            {
                // TODO: Add order value to log
                await this.logger.WriteWarningAsync(SERVICE_NAME, "", "", "Order does not contain any prices. Skipping.");
                return; // Skipping empty orders
            }

            // Calculate order min/max
            double extremPrice = order.Prices.Select(vp => vp.Price).Aggregate((extPrice, curPrice) => {
                return (order.IsBuy) ? Math.Max(extPrice, curPrice) : Math.Min(extPrice, curPrice);
            });

            // Compare new order with current min/max
            Quote currentQuote;
            bool isUpdated = false;
            string key = order.AssetPair + (order.IsBuy ? "buy" : "sell");
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
            else if ((order.IsBuy && extremPrice > currentQuote.Price)
                    || (!order.IsBuy && extremPrice < currentQuote.Price))
            {
                // Update quote
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
            if (string.IsNullOrEmpty(order.AssetPair))
            {
                errors.Add(string.Format("Invalid 'AssetPair': '{0}'", order.AssetPair ?? ""));
            }
            if (order.Timestamp == DateTime.MinValue || order.Timestamp == DateTime.MaxValue)
            {
                errors.Add(string.Format("Invalid 'Timestamp': '{0}'", order.AssetPair ?? ""));
            }
            if (order.Prices == null)
            {
                errors.Add("Invalid 'Price': null");
            }

            return errors;
        }
    }
}
