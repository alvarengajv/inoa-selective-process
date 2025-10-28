using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Domain.Entities
{
    public class MonitoredAsset
    {
        public string Ticker { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal BuyThreshold { get; set; }
        public decimal SellThreshold { get; set; }
    }
}
