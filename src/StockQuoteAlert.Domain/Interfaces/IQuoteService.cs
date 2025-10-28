using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Domain.Interfaces
{
    public interface IQuoteService
    {
        Task<decimal> GetQuoteAsync(string ticker);
    }
}
