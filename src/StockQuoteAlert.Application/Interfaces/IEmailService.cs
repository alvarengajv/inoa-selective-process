using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAlertAsync(string subject, string body);
    }
}
