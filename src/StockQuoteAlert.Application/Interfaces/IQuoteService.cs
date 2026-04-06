namespace StockQuoteAlert.Application.Interfaces
{
    public interface IQuoteService
    {
        Task<decimal> GetQuoteAsync(string ticker);
    }
}
