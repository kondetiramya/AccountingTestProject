using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Models;
using Moq;
using System;
using System.Collections.Generic;

namespace Repository.UnitTests.Common;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() => new Fixture()
        .Customize(new AutoMoqCustomization() { ConfigureMembers = false })
        .Customize(new InvoiceCustomization()))
    {
    }
}
