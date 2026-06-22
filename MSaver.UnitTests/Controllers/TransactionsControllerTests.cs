using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Contracts.Transactions;
using MSaver.Api.Controllers;
using MSaver.Application.Abstractions.Services;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.Transfer;
using MSaver.Application.Features.Transactions.Update;

namespace MSaver.UnitTests.Controllers;

public sealed class TransactionsControllerTests
{
    private readonly Mock<ITransactionService> _transactionService = new();

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
    public async Task GetTransferRate_ShouldReturnBadRequestAndSkipService_WhenRequestIsInvalid()
    {
        var sut = CreateSut();

        var result = await sut.GetTransferRate(Guid.Empty, Guid.Empty, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        _transactionService.Verify(
            x => x.GetTransferRateAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void DeleteTransfer_ShouldExposeTransferDeleteRoute()
    {
        var method = typeof(TransactionsController).GetMethod(nameof(TransactionsController.DeleteTransfer));

        method.Should().NotBeNull();

        var templates = method!
            .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: false)
            .Cast<HttpDeleteAttribute>()
            .Select(x => x.Template);

        templates.Should().Contain("transfers/{transferId:guid}");
    }

    [Fact]
    public void DeleteTransfer_ShouldExposeSingularTransferDeleteRoute()
    {
        var method = typeof(TransactionsController).GetMethod(nameof(TransactionsController.DeleteTransfer));

        method.Should().NotBeNull();

        var templates = method!
            .GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: false)
            .Cast<HttpDeleteAttribute>()
            .Select(x => x.Template);

        templates.Should().Contain("transfer/{transferId:guid}");
    }

    private TransactionsController CreateSut()
    {
        return new TransactionsController(
            _transactionService.Object,
            Mock.Of<IValidator<GetTransactionsRequest>>(),
            new GetTransferRateRequestValidator(),
            Mock.Of<IValidator<CreateTransactionRequest>>(),
            Mock.Of<IValidator<CreateTransferRequest>>(),
            Mock.Of<IValidator<UpdateTransactionRequest>>());
    }
}
