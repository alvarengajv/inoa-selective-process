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
                    writer.WriteUsage();
                    return 1;
                }

                var ticker = args[0];

                if (!ticker.EndsWith(".SA", StringComparison.OrdinalIgnoreCase))
                {
                    ticker = $"{ticker}.SA";
                    writer.WriteHint($"Sufixo .SA adicionado automaticamente: {ticker}");
                }

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

                if (sellThreshold <= buyThreshold)
                {
                    writer.WriteError("O preço de venda deve ser maior que o preço de compra.");
                    writer.WriteHint($"Venda: {sellThreshold:N2} | Compra: {buyThreshold:N2}");
                    return 1;
                }

                var host = CreateHostBuilder(args).Build();

                var emailService = host.Services.GetRequiredService<IEmailService>();

                try
                {
                    writer.WriteInfo("Validando configuração SMTP...");
                    await emailService.ValidateSmtpConfigurationAsync();
                    writer.WriteSuccess("Configuração SMTP válida.");
                    writer.WriteInfo("");
                }
                catch (Exception ex)
                {
                    writer.WriteError($"Configuração SMTP inválida: {ex.Message}");
                    writer.WriteHint("Verifique as configurações de e-mail no appsettings.json.");
                    return 1;
                }

                var monitoringService = host.Services.GetRequiredService<IMonitoringService>();

                var assetDto = new MonitoredAssetDto(ticker, buyThreshold, sellThreshold);

                writer.WriteHeader(ticker, buyThreshold, sellThreshold);

                writer.WriteHint("Pressione Ctrl+C para encerrar o monitoramento.");

                using var cts = new CancellationTokenSource();
                System.Console.CancelKeyPress += (sender, e) =>
                {
                    writer.WriteShutdown("\nEncerrando monitoramento...");
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
