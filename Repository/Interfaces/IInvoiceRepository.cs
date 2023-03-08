using System;
using System.Collections.Generic;

namespace Repository.Interfaces
{
    public interface IInvoiceRepository
    {
        public decimal? GetTotal(int invoiceId);
        public decimal GetTotalOfUnpaid();
        public IReadOnlyDictionary<string, long> GetItemsReport(DateTime? from, DateTime? to);
    }
}
