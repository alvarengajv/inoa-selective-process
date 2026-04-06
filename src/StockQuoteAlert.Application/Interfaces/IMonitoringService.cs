using StockQuoteAlert.Application.DTOs;

namespace StockQuoteAlert.Application.Interfaces
{
    public interface IMonitoringService
    {
        Task StartMonitoringAsync(MonitoredAssetDto dto, CancellationToken cancellationToken = default);
    }
}
