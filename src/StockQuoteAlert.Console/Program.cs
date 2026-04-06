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
            var writer = new ConsoleWriter();

            try
            {
                if (args.Length < 3)
                {
                    writer.WriteError("Argumentos insuficientes.");
                    System.Console.ForegroundColor = ConsoleColor.White;
                    System.Console.WriteLine("  Uso     : StockQuoteAlert <ticker> <preço_venda> <preço_compra>");
                    System.Console.WriteLine("  Exemplo : StockQuoteAlert PETR4.SA 30.50 28.00");
                    System.Console.ResetColor();
                    return 1;
                }

                var ticker = args[0];

                if (!decimal.TryParse(args[1], out var sellThreshold))
                {
                    writer.WriteError($"O preço de venda '{args[1]}' não é um número válido.");
                    return 1;
                }

                if (!decimal.TryParse(args[2], out var buyThreshold))
                {
                    writer.WriteError($"O preço de compra '{args[2]}' não é um número válido.");
                    return 1;
                }

                var host = CreateHostBuilder(args).Build();

                var emailService = host.Services.GetRequiredService<IEmailService>();

                try
                {
                    writer.WriteInfo("Validando configuração SMTP...");
                    await emailService.ValidateSmtpConfigurationAsync();
                    writer.WriteSuccess("Configuração SMTP válida.");
                    System.Console.WriteLine();
                }
                catch (Exception ex)
                {
                    writer.WriteError($"Configuração SMTP inválida: {ex.Message}");
                    System.Console.ForegroundColor = ConsoleColor.DarkGray;
                    System.Console.WriteLine("  Verifique as configurações de e-mail no appsettings.json.");
                    System.Console.ResetColor();
                    return 1;
                }

                var monitoringService = host.Services.GetRequiredService<IMonitoringService>();

                var assetDto = new MonitoredAssetDto
                {
                    Ticker = ticker,
                    BuyThreshold = buyThreshold,
                    SellThreshold = sellThreshold
                };

                writer.WriteHeader(ticker, buyThreshold, sellThreshold);

                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.WriteLine("  Pressione Ctrl+C para encerrar o monitoramento.");
                System.Console.ResetColor();

                using var cts = new CancellationTokenSource();
                System.Console.CancelKeyPress += (sender, e) =>
                {
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                    System.Console.WriteLine("\nEncerrando monitoramento...");
                    System.Console.ResetColor();
                    e.Cancel = true;
                    cts.Cancel();
                };

                await monitoringService.StartMonitoringAsync(assetDto, cts.Token);

                return 0;
            }
            catch (Exception ex)
            {
                writer.WriteError($"Erro fatal: {ex.Message}");
                return 1;
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment.EnvironmentName;
                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables()
                          .AddUserSecrets<Program>(optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplicationServices(context.Configuration);
                    services.AddInfrastructureServices(context.Configuration);
                });
    }
}
