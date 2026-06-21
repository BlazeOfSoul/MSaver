using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Controllers;

namespace MSaver.UnitTests.Controllers;

public sealed class TransactionsControllerTests
{
    [Fact]
    public void GetTransferRate_ShouldExposeTransferRateRoute()
    {
        var method = typeof(TransactionsController).GetMethod(nameof(TransactionsController.GetTransferRate));

        method.Should().NotBeNull();

        var attribute = method!
            .GetCustomAttributes(typeof(HttpGetAttribute), inherit: false)
            .Should()
            .ContainSingle()
            .Subject
            .Should()
            .BeOfType<HttpGetAttribute>()
            .Subject;

        attribute.Template.Should().Be("transfer-rate");
    }

    [Fact]
    public void DeleteTransfer_ShouldExposeTransferDeleteRoute()
    {
        var method = typeof(TransactionsController).GetMethod(nameof(TransactionsController.DeleteTransfer));

        method.Should().NotBeNull();

        var attribute = method!
            .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: false)
            .Should()
            .ContainSingle()
            .Subject
            .Should()
            .BeOfType<HttpDeleteAttribute>()
            .Subject;

        attribute.Template.Should().Be("transfers/{transferId:guid}");
    }
}
