using System.Threading.Tasks;
using Lykke.Domain.Prices.Model;

namespace QuoteFeed.Core.Contracts
{
    public interface IQuotePublisher
    {
        Task Publish(Quote quote);
    }
}
