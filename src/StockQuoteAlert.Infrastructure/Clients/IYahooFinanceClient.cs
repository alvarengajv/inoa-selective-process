using System.Collections.Generic;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockQuoteAlert.Infrastructure.ExternalServices
{
    public interface IYahooFinanceClient
    {
        Task<IReadOnlyDictionary<string, Security>> QueryAsync(string ticker);
    }
}
