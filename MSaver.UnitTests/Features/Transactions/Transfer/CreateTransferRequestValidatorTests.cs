using MSaver.Application.Common.Validation;
using MSaver.Application.Features.Transactions.Transfer;

namespace MSaver.UnitTests.Features.Transactions.Transfer;

public sealed class CreateTransferRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnToAccountError_WhenAccountsAreSame()
    {
        var sut = new CreateTransferRequestValidator();
        var accountId = Guid.NewGuid();
        var request = TransactionTestData.CreateTransferRequest(
            fromAccountId: accountId,
            toAccountId: accountId);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(CreateTransferRequest.ToAccountId) &&
            x.ErrorMessage == ValidationMessages.DifferentAccountsRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_ShouldReturnError_WhenRateIsNotPositive(decimal rate)
    {
        var sut = new CreateTransferRequestValidator();
        var request = TransactionTestData.CreateTransferRequest(rate: rate);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(CreateTransferRequest.Rate) &&
            x.ErrorMessage == ValidationMessages.MustBePositive);
    }
}
