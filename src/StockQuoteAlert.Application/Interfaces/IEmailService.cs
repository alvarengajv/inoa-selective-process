namespace StockQuoteAlert.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAlertAsync(string subject, string body);
        Task ValidateSmtpConfigurationAsync();
    }
}
