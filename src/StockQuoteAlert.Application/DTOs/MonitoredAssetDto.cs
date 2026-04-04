using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Application.DTOs
{
    public class MonitoredAssetDto
    {
        public string Ticker { get; set; }
        public decimal BuyThreshold { get; set; }
        public decimal SellThreshold { get; set; }
    }
}
