using System;
using StockQuoteAlert.Application.Interfaces;
using YahooFinanceApi;

namespace StockQuoteAlert.Infrastructure.ExternalServices
{
    public class QuoteService : IQuoteService
    {
        private readonly IYahooFinanceClient _yahooClient;

        public QuoteService(IYahooFinanceClient yahooClient)
        {
            _yahooClient = yahooClient ?? throw new ArgumentNullException(nameof(yahooClient));
        }

        public async Task<decimal> GetQuoteAsync(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("O ticker não pode ser vazio.", nameof(ticker));

            try
            {
                var securities = await _yahooClient.QueryAsync(ticker);

                if (!securities.TryGetValue(ticker, out var security))
                    throw new InvalidOperationException($"Não foi possível encontrar dados para o ticker {ticker}.");
                dynamic price = security[Field.RegularMarketPrice];

                if (price == null)
                    throw new InvalidOperationException($"Preço inválido retornado para o ticker {ticker}.");

                return (decimal)(double)price;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Erro ao obter cotação do ticker {ticker}: {ex.Message}", ex);
            }
        }
    }
}
