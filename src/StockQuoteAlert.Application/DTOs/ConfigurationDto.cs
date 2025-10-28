using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Application.DTOs
{
    public class ConfigurationDto
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool EnableSsl { get; set; }
        public string RecipientEmail { get; set; }
        public int MonitoringIntervalSeconds { get; set; }
    }
}
