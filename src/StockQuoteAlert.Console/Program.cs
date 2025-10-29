using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockQuoteAlert.Console.Extensions;
using StockQuoteAlert.Application.DTOs;
using StockQuoteAlert.Application.Interfaces;

namespace StockQuoteAlert.Console
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Uso: StockQuoteAlert <ticker> <preço_venda> <preço_compra>");
                    Console.WriteLine("Exemplo: StockQuoteAlert PETR4.SA 30.50 28.00");
                    return 1;
                }

                var ticker = args[0];

                if (!decimal.TryParse(args[1], out var sellThreshold))
                {
                    Console.WriteLine($"Erro: O preço de venda '{args[1]}' não é um número válido.");
                    return 1;
                }

                if (!decimal.TryParse(args[2], out var buyThreshold))
                {
                    Console.WriteLine($"Erro: O preço de compra '{args[2]}' não é um número válido.");
                    return 1;
                }

                var host = CreateHostBuilder(args).Build();

                var monitoringService = host.Services.GetRequiredService<IMonitoringService>();

                var assetDto = new MonitoredAssetDto
                {
                    Ticker = ticker,
                    BuyThreshold = buyThreshold,
                    SellThreshold = sellThreshold
                };

                Console.WriteLine("=== Stock Quote Alert ===");
                Console.WriteLine("Pressione Ctrl+C para encerrar o monitoramento.\n");

                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("\nEncerrando monitoramento...");
                    e.Cancel = true;
                    cts.Cancel();
                };

                await monitoringService.StartMonitoringAsync(assetDto, cts.Token);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro fatal: {ex.Message}");
                return 1;
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplicationServices(context.Configuration);
                    services.AddInfrastructureServices(context.Configuration);
                });
    }
}
