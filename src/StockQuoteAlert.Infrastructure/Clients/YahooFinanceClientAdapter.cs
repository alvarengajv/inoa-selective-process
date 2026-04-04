using System.Collections.Generic;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockQuoteAlert.Infrastructure.ExternalServices
{
    public sealed class YahooFinanceClientAdapter : IYahooFinanceClient
    {
        public async Task<IReadOnlyDictionary<string, Security>> QueryAsync(string ticker)
        {
            return await Yahoo
                .Symbols(ticker)
                .Fields(Field.Symbol, Field.RegularMarketPrice)
                .QueryAsync();
        }
    }
}
