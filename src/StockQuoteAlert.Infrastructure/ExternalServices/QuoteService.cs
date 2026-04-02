using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StockQuoteAlert.Application.Interfaces;
using YahooFinanceApi;

namespace StockQuoteAlert.Infrastructure.ExternalServices
{
    public class QuoteService : IQuoteService
    {
        public async Task<decimal> GetQuoteAsync(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("O ticker não pode ser vazio.", nameof(ticker));

            try
            {
                var securities = await Yahoo
                    .Symbols(ticker)
                    .Fields(Field.Symbol, Field.RegularMarketPrice)
                    .QueryAsync();

                if (!securities.ContainsKey(ticker))
                    throw new InvalidOperationException($"Não foi possível encontrar dados para o ticker {ticker}.");

                var security = securities[ticker];
                var price = security[Field.RegularMarketPrice];
                decimal priceDecimal = 0;

                if (price == null || !decimal.TryParse(price.ToString(), out priceDecimal))
                    throw new InvalidOperationException($"Preço inválido retornado para o ticker {ticker}.");

                return priceDecimal;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Erro ao obter cotação do ticker {ticker}: {ex.Message}", ex);
            }
        }
    }
}
