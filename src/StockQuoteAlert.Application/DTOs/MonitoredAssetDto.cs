namespace StockQuoteAlert.Application.DTOs
{
    public record MonitoredAssetDto(string Ticker, decimal BuyThreshold, decimal SellThreshold);
}
