using FluentAssertions;
using Models;
using Repository.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Repository.UnitTests;

public class InvoiceRepositoryTests
{
    [Fact]
    public void GetTotal_NoItemsInRepository_ReturnsNull()
    {
        IQueryable<Invoice> invoices = Enumerable.Empty<Invoice>().AsQueryable();
        var sut = new InvoiceRepository(invoices);

        sut.GetTotal(1).Should().BeNull();
    }

    [Theory]
    [AutoMoqData]
    public void GetTotal_ItemNotFound_ReturnsNull(IEnumerable<Invoice> invoices)
    {
        var sut = new InvoiceRepository(invoices.AsQueryable());

        sut.GetTotal(-1).Should().BeNull();
    }

    [Theory]
    [AutoMoqData]
    public void GetTotal_ItemFound_ReturnsTotal(IEnumerable<Invoice> invoices)
    {
        var firstInvoice = invoices.First();
        decimal expectedTotal = 0;
        foreach (var item in firstInvoice.InvoiceItems)
        {
            expectedTotal += item.Count * item.Price;
        }

        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualTotal = sut.GetTotal(firstInvoice.Id);
        actualTotal.Should().Be(expectedTotal);
    }

    [Theory]
    [AutoMoqData]
    public void GetTotal_ItemFoundWithEmptyInvoiceItems_ReturnsNull(IEnumerable<Invoice> invoices)
    {
        var firstInvoice = invoices.First();
        firstInvoice.InvoiceItems.Clear();

        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualTotal = sut.GetTotal(firstInvoice.Id);
        actualTotal.Should().BeNull();
    }

    [Theory]
    [AutoMoqData]
    public void GetTotal_OverflowException_ReturnsNull(Invoice invoice, InvoiceItem invoiceItem)
    {
        invoice.InvoiceItems.Clear();
        invoiceItem.Price = decimal.MaxValue;
        invoiceItem.Count = int.MaxValue;
        invoice.InvoiceItems.Add(invoiceItem);

        var sut = new InvoiceRepository(new[] { invoice}.AsQueryable());
        var actualTotal = sut.GetTotal(invoice.Id);
        actualTotal.Should().BeNull();
    }

    [Fact]
    public void GetTotalOfUnpaid_NoInvoices_ShouldReturnZero()
    {
        var sut = new InvoiceRepository(Enumerable.Empty<Invoice>().AsQueryable());
        var actualTotal = sut.GetTotalOfUnpaid();
        actualTotal.Should().Be(0);
    }

    [Theory]
    [AutoMoqData]
    public void GetTotalOfUnpaid_InvoicesWithEmptyInvoiceItems_ReturnsZero(IEnumerable<Invoice> paidInvoices, IEnumerable<Invoice> unpaidInvoices)
    {
        foreach (var invoice in unpaidInvoices)
        {
            invoice.AcceptanceDate = null;
            invoice.InvoiceItems.Clear();
        }
        var sut = new InvoiceRepository(paidInvoices.Union(unpaidInvoices).AsQueryable());
        var actualTotal = sut.GetTotalOfUnpaid();
        actualTotal.Should().Be(0);
    }

    [Theory]
    [AutoMoqData]
    public void GetTotalOfUnpaid_ReturnsTotal(IEnumerable<Invoice> paidInvoices, IEnumerable<Invoice> unpaidInvoices)
    {
        decimal expectedTotal = 0;
        foreach (var invoice in unpaidInvoices)
        {
            invoice.AcceptanceDate = null;
            expectedTotal += invoice.InvoiceItems.Select(item => item.Count * item.Price).Sum();
        }
        var sut = new InvoiceRepository(paidInvoices.Union(unpaidInvoices).AsQueryable());
        var actualTotal = sut.GetTotalOfUnpaid();
        actualTotal.Should().Be(expectedTotal);
    }

    [Theory]
    [AutoMoqData]
    public void GetTotalOfUnpaid_NoUnpaidInvoiceItems_ReturnsZero(IEnumerable<Invoice> paidInvoices)
    {
        var sut = new InvoiceRepository(paidInvoices.AsQueryable());
        var actualTotal = sut.GetTotalOfUnpaid();
        actualTotal.Should().Be(0);
    }

    [Theory]
    [AutoMoqData]
    public void GetTotalOfUnpaid_OverflowException_ReturnsZero(Invoice invoice, InvoiceItem invoiceItem)
    {
        invoice.InvoiceItems.Clear();
        invoiceItem.Price = decimal.MaxValue;
        invoiceItem.Count = int.MaxValue;
        invoice.InvoiceItems.Add(invoiceItem);

        var sut = new InvoiceRepository(new[] { invoice }.AsQueryable());
        var actualTotal = sut.GetTotalOfUnpaid();
        actualTotal.Should().Be(0);
    }

    [Fact]
    public void GetItemsReport_NoInvoices_ShouldReturnEmpty()
    {
        var sut = new InvoiceRepository(Enumerable.Empty<Invoice>().AsQueryable());
        var actualResult = sut.GetItemsReport(null, null);
        actualResult.Should().BeEmpty();
    }

    [Theory]
    [AutoMoqData]
    public void GetItemsReport_InvoicesWithEmptyInvoiceItems_ReturnsEmpty(IEnumerable<Invoice> invoices)
    {
        foreach (var invoice in invoices)
        {
            invoice.InvoiceItems.Clear();
        }

        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualResult = sut.GetItemsReport(null, null);
        actualResult.Should().BeEmpty();
    }

    [Theory]
    [AutoMoqData]
    public void GetItemsReport_WithNullStartAndEndDate_ReturnsReportOfAllItems(IEnumerable<Invoice> invoices)
    {
        var expectedResult = invoices
                            .SelectMany(invoice => invoice.InvoiceItems)
                            .GroupBy(item => item.Name)
                            .ToDictionary(g => g.Key, g => g.Sum(item => item.Count));
        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualResult = sut.GetItemsReport(null, null);
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [AutoMoqData]
    public void GetItemsReport_ReturnsReportFromStartDate(IEnumerable<Invoice> invoices)
    {
        var startDate = invoices.OrderBy(invoice => invoice.CreationDate).First().CreationDate.AddDays(1);
        var expectedResult = invoices
                            .Where(invoice => invoice.CreationDate >= startDate)
                            .SelectMany(invoice => invoice.InvoiceItems)
                            .GroupBy(item => item.Name)
                            .ToDictionary(g => g.Key, g => g.Sum(item => item.Count));
        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualResult = sut.GetItemsReport(startDate, null);
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [AutoMoqData]
    public void GetItemsReport_ReturnsReportToEndDate(IEnumerable<Invoice> invoices)
    {
        var endDate = invoices.OrderBy(invoice => invoice.CreationDate).Last().CreationDate.AddDays(-1);
        var expectedResult = invoices
                            .Where(invoice => invoice.CreationDate <= endDate)
                            .SelectMany(invoice => invoice.InvoiceItems)
                            .GroupBy(item => item.Name)
                            .ToDictionary(g => g.Key, g => g.Sum(item => item.Count));
        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualResult = sut.GetItemsReport(null, endDate);
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [AutoMoqData]
    public void GetItemsReport_ReturnsReportFromStartToEndDate(
        [MinLength(10), MaxLength(10)] IEnumerable<Invoice> invoices)
    {
        var startDate = invoices.OrderBy(invoice => invoice.CreationDate).First().CreationDate.AddDays(2);
        var endDate = invoices.OrderBy(invoice => invoice.CreationDate).Last().CreationDate.AddDays(-2);
        var expectedResult = invoices
                            .Where(invoice => invoice.CreationDate >= startDate && invoice.CreationDate <= endDate)
                            .SelectMany(invoice => invoice.InvoiceItems)
                            .GroupBy(item => item.Name)
                            .ToDictionary(g => g.Key, g => g.Sum(item => item.Count));
        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualResult = sut.GetItemsReport(startDate, endDate);
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [AutoMoqData]
    public void GetItemsReport_StartDateGreaterThanEndDate_ReturnsEmpty(
        [MinLength(10), MaxLength(10)] IEnumerable<Invoice> invoices)
    {
        var endDate = invoices.OrderBy(invoice => invoice.CreationDate).First().CreationDate.AddDays(2);
        var startDate = invoices.OrderBy(invoice => invoice.CreationDate).Last().CreationDate.AddDays(-2);

        var sut = new InvoiceRepository(invoices.AsQueryable());
        var actualResult = sut.GetItemsReport(startDate, endDate);
        actualResult.Should().BeEmpty();
    }
}