using System.Threading.Tasks;
using QuoteFeed.Core.Model;

namespace QuoteFeed.Core.Contracts
{
    public interface IQuotePublisher
    {
        Task Publish(Quote quote);
    }
}
