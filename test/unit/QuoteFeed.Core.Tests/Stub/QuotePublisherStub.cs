using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuoteFeed.Core.Contracts;
using Lykke.Domain.Prices.Model;

namespace QuoteFeed.Core.Tests.Stub
{
    /// <summary>
    /// Stub for the interface IQuotePublisher
    /// </summary>
    public class QuotePublisherStub : IQuotePublisher
    {
        private Func<Quote, Task> publishDelegate;
        private List<Quote> published = new List<Quote>();

        public IReadOnlyList<Quote> Published { get { return this.published; } }

        public QuotePublisherStub(Func<Quote, Task> publishDelegate = null)
        {
            this.publishDelegate = publishDelegate;
        }

        public Task Publish(Quote quote)
        {
            this.published.Add(quote);

            if (this.publishDelegate != null)
            {
                return this.publishDelegate(quote);
            }
            else
            {
                return Task.FromResult(0);
            }
        }
    }
}
