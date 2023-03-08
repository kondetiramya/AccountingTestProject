using AutoFixture;
using Models;
using System;
using System.Collections.Generic;

namespace Repository.UnitTests.Common;

public class InvoiceCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Invoice>(composer =>
        composer.FromFactory<int, DateTime, IList<InvoiceItem>>(
            (id, creationDate, invoiceItems) =>
            {
                var invoice = new Invoice
                {
                    Id = id,
                    Description = $"desc{Guid.NewGuid()}",
                    Number = $"number{Guid.NewGuid()}",
                    Seller = $"seller{Guid.NewGuid()}",
                    Buyer = $"buyer{Guid.NewGuid()}",
                    CreationDate = creationDate,
                    AcceptanceDate = creationDate.AddDays(Random.Shared.NextInt64(3, 15)),
                };
                ((List<InvoiceItem>)invoice.InvoiceItems).AddRange(invoiceItems);
                return invoice;
            }).OmitAutoProperties());
    }
}
