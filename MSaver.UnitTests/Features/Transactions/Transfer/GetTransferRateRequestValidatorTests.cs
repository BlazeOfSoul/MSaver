using MSaver.Application.Common.Validation;
using MSaver.Application.Features.Transactions.Transfer;

namespace MSaver.UnitTests.Features.Transactions.Transfer;

public sealed class GetTransferRateRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnErrors_WhenAccountIdsAreEmpty()
    {
        var sut = new GetTransferRateRequestValidator();
        var request = new GetTransferRateRequest(Guid.Empty, Guid.Empty);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(GetTransferRateRequest.FromAccountId) &&
            x.ErrorMessage == ValidationMessages.InvalidId);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(GetTransferRateRequest.ToAccountId) &&
            x.ErrorMessage == ValidationMessages.InvalidId);
    }

    [Fact]
    public void Validate_ShouldReturnToAccountError_WhenAccountsAreSame()
    {
        var sut = new GetTransferRateRequestValidator();
        var accountId = Guid.NewGuid();
        var request = new GetTransferRateRequest(accountId, accountId);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(GetTransferRateRequest.ToAccountId) &&
            x.ErrorMessage == ValidationMessages.DifferentAccountsRequired);
    }
}
