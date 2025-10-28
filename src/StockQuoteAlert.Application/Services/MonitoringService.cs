using System;
using System.Collections.Generic;
using System.Text;
using StockQuoteAlert.Domain.Entities;
using StockQuoteAlert.Domain.Interfaces;
using StockQuoteAlert.Application.DTOs;
using StockQuoteAlert.Application.Interfaces;


namespace StockQuoteAlert.Application.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly IEmailService _emailService;
        private readonly IQuoteService _quoteService;
        private readonly int _monitoringIntervalSeconds;

        public MonitoringService(
            IEmailService emailService,
            IQuoteService quoteService,
            int monitoringIntervalSeconds)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
            _monitoringIntervalSeconds = monitoringIntervalSeconds > 0 ? monitoringIntervalSeconds : throw new ArgumentException("Intervalo deve ser maior que zero.", nameof(monitoringIntervalSeconds));
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

            Console.WriteLine($"Ativo a ser monitorado: {asset.Ticker}");
            Console.WriteLine($"Preço de referência para venda: {asset.SellThreshold}");
            Console.WriteLine($"Preço de referência para compra: {asset.BuyThreshold}");
            Console.WriteLine(new string('-', 50));

            // Variáveis para controlar os alertas
            bool buyAlertSent = false;
            bool sellAlertSent = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentPrice = await _quoteService.GetQuoteAsync(asset.Ticker);
                    asset.UpdatePrice(currentPrice);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {asset.Ticker}: {currentPrice:C}");

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
                    Console.WriteLine($"\nMonitoramento de {asset.Ticker} cancelado.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao monitorar {asset.Ticker}: {ex.Message}");
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
            Console.WriteLine($"✓ Alerta de COMPRA enviado para {asset.Ticker}");
        }

        private async Task SendSellAlertAsync(MonitoredAsset asset)
        {
            var subject = $"Alerta de Venda - {asset.Ticker}";
            var body = $"O ativo {asset.Ticker} atingiu o preço de {asset.CurrentPrice:C}, " +
                       $"que está acima ou igual ao limite de venda de {asset.SellThreshold:C}. " +
                       $"Considere vender!";

            await _emailService.SendAlertAsync(subject, body);
            Console.WriteLine($"✓ Alerta de VENDA enviado para {asset.Ticker}");
        }
    }
}
