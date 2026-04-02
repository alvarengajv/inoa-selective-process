using System;
using System.Collections.Generic;
using System.Text;

namespace StockQuoteAlert.Application.Interfaces
{
    public interface IQuoteService
    {
        Task<decimal> GetQuoteAsync(string ticker);
    }
}
