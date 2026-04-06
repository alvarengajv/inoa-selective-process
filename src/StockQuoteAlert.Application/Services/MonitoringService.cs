using System;
using StockQuoteAlert.Domain.Entities;
using StockQuoteAlert.Application.DTOs;
using StockQuoteAlert.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace StockQuoteAlert.Application.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly IEmailService _emailService;
        private readonly IQuoteService _quoteService;
        private readonly int _monitoringIntervalSeconds;
        private readonly ILogger<MonitoringService> _logger;

        public MonitoringService(
            IEmailService emailService,
            IQuoteService quoteService,
            int monitoringIntervalSeconds,
            ILogger<MonitoringService> logger)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
            _monitoringIntervalSeconds = monitoringIntervalSeconds > 0 ? monitoringIntervalSeconds : throw new ArgumentException("Intervalo deve ser maior que zero.", nameof(monitoringIntervalSeconds));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartMonitoringAsync(MonitoredAssetDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var asset = MonitoredAsset.Create(dto.Ticker, dto.BuyThreshold, dto.SellThreshold);

            // Variáveis para controlar os alertas
            bool buyAlertSent = false;
            bool sellAlertSent = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentPrice = await _quoteService.GetQuoteAsync(asset.Ticker);
                    asset.UpdatePrice(currentPrice);

                    _logger.LogInformation("[{Time}] {Ticker}: R$ {Price:N2}", DateTime.Now.ToString("HH:mm:ss"), asset.Ticker, currentPrice);

                    // Alerta de compra
                    if (asset.ShouldTriggerBuyAlert())
                    {
                        if (!buyAlertSent)
                        {
                            await SendAlertAsync(asset, "Compra", "comprar", "abaixo", asset.BuyThreshold);
                            buyAlertSent = true;
                            sellAlertSent = false; // Resetar o alerta de venda
                        }
                    }
                    // Alerta de venda
                    else if (asset.ShouldTriggerSellAlert())
                    {
                        if (!sellAlertSent)
                        {
                            await SendAlertAsync(asset, "Venda", "vender", "acima", asset.SellThreshold);
                            sellAlertSent = true;
                            buyAlertSent = false; // Resetar o alerta de compra
                        }
                    }
                    // Resetar os alertas quando o preço estiver entre os limites
                    else
                    {
                        buyAlertSent = false;
                        sellAlertSent = false;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_monitoringIntervalSeconds), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Monitoramento de {Ticker} encerrado.", asset.Ticker);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao monitorar {Ticker}: {Message}", asset.Ticker, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(_monitoringIntervalSeconds), cancellationToken);
                }
            }
        }

        private async Task SendAlertAsync(MonitoredAsset asset, string tipo, string acao, string direcao, decimal threshold)
        {
            var subject = $"Alerta de {tipo} - {asset.Ticker}";
            var body = $"O ativo {asset.Ticker} atingiu o preço de R$ {asset.CurrentPrice:N2}, " +
                       $"que está {direcao} ou igual ao limite de {tipo.ToLowerInvariant()} de R$ {threshold:N2}. " +
                       $"Considere {acao}!";

            await _emailService.SendAlertAsync(subject, body);
            _logger.LogInformation("Alerta de {Tipo} enviado para {Ticker}", tipo.ToUpperInvariant(), asset.Ticker);
        }
    }
}
