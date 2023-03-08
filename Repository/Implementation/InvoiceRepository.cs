using Models;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IQueryable<Invoice> _invoices;
        public InvoiceRepository(IQueryable<Invoice> invoices)
        {
            _invoices = invoices ?? throw new ArgumentNullException(nameof(invoices));
        }
        public IReadOnlyDictionary<string, long> GetItemsReport(DateTime? from, DateTime? to)
        {
            try
            {
                return _invoices
                    .Where(invoice => (from == null || invoice.CreationDate >= from) && (to == null || invoice.CreationDate <= to))
                    .SelectMany(invoice => invoice.InvoiceItems)
                    .GroupBy(item => item.Name)
                    .ToDictionary(g => g.Key, g => g.Sum(item => item.Count));
            }
            catch (OverflowException)
            {
                return default;
            }
        }

        public decimal? GetTotal(int invoiceId)
        {
            try
            {
                decimal? total = _invoices
                            .Where(invoice => invoice.Id == invoiceId)
                            .Select(invoice => invoice.InvoiceItems)
                            .Select(item => item.Sum(item => item.Price * item.Count))
                            .Sum();
                return total == 0 ? default : total;
            }
            catch (OverflowException)
            {
                return default;
            }
        }

        public decimal GetTotalOfUnpaid()
        {
            try
            {
                return _invoices
                    .Where(invoice => invoice.AcceptanceDate == null)
                    .Select(invoice => invoice.InvoiceItems)
                    .Select(item => item.Sum(s => s.Price * s.Count))
                    .Sum();
            }
            catch (OverflowException)
            {
                return default;
            }
        }
    }
}
