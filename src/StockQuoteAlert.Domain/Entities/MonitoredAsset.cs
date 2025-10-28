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

        private MonitoredAsset()
        {
        }

        public static MonitoredAsset Create(string ticker, decimal buyThreshold, decimal sellThreshold)
        {
            ValidateParameters(ticker, buyThreshold, sellThreshold);

            return new MonitoredAsset
            {
                Ticker = ticker.ToUpperInvariant(),
                BuyThreshold = buyThreshold,
                SellThreshold = sellThreshold,
                CurrentPrice = 0
            };
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("O preço deve ser maior que zero.", nameof(newPrice));

            CurrentPrice = newPrice;
        }

        public bool ShouldTriggerBuyAlert()
        {
            return CurrentPrice > 0 && CurrentPrice <= BuyThreshold;
        }

        public bool ShouldTriggerSellAlert()
        {
            return CurrentPrice > 0 && CurrentPrice >= SellThreshold;
        }

        private static void ValidateParameters(string ticker, decimal buyThreshold, decimal sellThreshold)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("O ticker não pode ser vazio.", nameof(ticker));

            if (buyThreshold <= 0)
                throw new ArgumentException("O limite de compra deve ser maior que zero.", nameof(buyThreshold));

            if (sellThreshold <= 0)
                throw new ArgumentException("O limite de venda deve ser maior que zero.", nameof(sellThreshold));

            if (sellThreshold <= buyThreshold)
                throw new ArgumentException("O limite de venda deve ser maior que o limite de compra.", nameof(sellThreshold));
        }
    }
}
