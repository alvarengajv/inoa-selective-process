using System;
using System.Collections.Generic;
using System.Text;
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

            // Validação básica dos dados do DTO
            if (string.IsNullOrWhiteSpace(dto.Ticker))
            {
                throw new ArgumentException("Ticker não pode ser vazio", nameof(dto));
            }

            if (dto.BuyThreshold <= 0)
            {
                throw new ArgumentException("Limite de compra deve ser maior que zero", nameof(dto));
            }

            if (dto.SellThreshold <= 0)
            {
                throw new ArgumentException("Limite de venda deve ser maior que zero", nameof(dto));
            }

            var asset = MonitoredAsset.Create(dto.Ticker, dto.BuyThreshold, dto.SellThreshold);

            _logger.LogInformation("Ativo a ser monitorado: {Ticker}", asset.Ticker);
            _logger.LogInformation("Preço de referência para venda: {SellThreshold}", asset.SellThreshold);
            _logger.LogInformation("Preço de referência para compra: {BuyThreshold}", asset.BuyThreshold);
            _logger.LogInformation("{Separator}", new string('-', 50));

            // Variáveis para controlar os alertas
            bool buyAlertSent = false;
            bool sellAlertSent = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentPrice = await _quoteService.GetQuoteAsync(asset.Ticker);
                    asset.UpdatePrice(currentPrice);

                    _logger.LogInformation("[{Time}] {Ticker}: {CurrentPrice:C}", DateTime.Now.ToString("HH:mm:ss"), asset.Ticker, currentPrice);

                    // Alerta de compra
                    if (asset.ShouldTriggerBuyAlert())
                    {
                        if (!buyAlertSent)
                        {
                            await SendBuyAlertAsync(asset);
                            buyAlertSent = true;
                            sellAlertSent = false; // Resetar o alerta de venda
                        }
                    }
                    // Alerta de venda
                    else if (asset.ShouldTriggerSellAlert())
                    {
                        if (!sellAlertSent)
                        {
                            await SendSellAlertAsync(asset);
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
                    _logger.LogInformation("\nMonitoramento de {Ticker} cancelado.", asset.Ticker);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao monitorar {Ticker}: {Message}", asset.Ticker, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(_monitoringIntervalSeconds), cancellationToken);
                }
            }
        }

        private async Task SendBuyAlertAsync(MonitoredAsset asset)
        {
            var subject = $"Alerta de Compra - {asset.Ticker}";
            var body = $"O ativo {asset.Ticker} atingiu o preço de {asset.CurrentPrice:C}, " +
                       $"que está abaixo ou igual ao limite de compra de {asset.BuyThreshold:C}. " +
                       $"Considere comprar!";

            await _emailService.SendAlertAsync(subject, body);
            _logger.LogInformation("✓ Alerta de COMPRA enviado para {Ticker}", asset.Ticker);
        }

        private async Task SendSellAlertAsync(MonitoredAsset asset)
        {
            var subject = $"Alerta de Venda - {asset.Ticker}";
            var body = $"O ativo {asset.Ticker} atingiu o preço de {asset.CurrentPrice:C}, " +
                       $"que está acima ou igual ao limite de venda de {asset.SellThreshold:C}. " +
                       $"Considere vender!";

            await _emailService.SendAlertAsync(subject, body);
            _logger.LogInformation("✓ Alerta de VENDA enviado para {Ticker}", asset.Ticker);
        }
    }
}
