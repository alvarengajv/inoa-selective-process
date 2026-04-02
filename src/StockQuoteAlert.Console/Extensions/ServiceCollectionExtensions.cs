using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockQuoteAlert.Application.Interfaces;
using StockQuoteAlert.Application.Services;
using StockQuoteAlert.Domain.Interfaces;
using StockQuoteAlert.Infrastructure.ExternalServices;

namespace StockQuoteAlert.Console.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var intervalSeconds = configuration.GetValue<int>("MonitoringSettings:IntervalSeconds");

            services.AddSingleton<IMonitoringService>(provider =>
            {
                var quoteService = provider.GetRequiredService<IQuoteService>();
                var emailService = provider.GetRequiredService<IEmailService>();
                var logger = provider.GetRequiredService<ILogger<MonitoringService>>();
                return new MonitoringService(emailService, quoteService, intervalSeconds, logger);
            });

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IQuoteService, QuoteService>();

            services.AddSingleton<IEmailService>(provider =>
            {
                var smtpServer = configuration["EmailSettings:SmtpServer"];
                var smtpPort = configuration.GetValue<int>("EmailSettings:SmtpPort");
                var username = configuration["EmailSettings:SmtpUsername"];
                var password = configuration["EmailSettings:SmtpPassword"];
                var enableSsl = configuration.GetValue<bool>("EmailSettings:EnableSsl");
                var recipientEmail = configuration["EmailSettings:RecipientEmail"];

                return new EmailService(smtpServer!, smtpPort, username!, password!, enableSsl, recipientEmail!);
            });

            return services;
        }
    }
}
