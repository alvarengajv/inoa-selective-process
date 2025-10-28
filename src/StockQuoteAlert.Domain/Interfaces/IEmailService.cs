using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendAlertAsync(string subject, string body);
    }
}
