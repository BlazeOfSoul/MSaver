using MSaver.Application.Common.Validation;
using MSaver.Application.Features.Transactions.Transfer;

namespace MSaver.UnitTests.Features.Transactions.Transfer;

public sealed class CreateTransferRequestValidatorTests
{
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
