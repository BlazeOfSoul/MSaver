using FluentValidation.Results;

using MSaver.Api.Common;
using MSaver.Domain.Common;

namespace MSaver.UnitTests.Api.Common;

public sealed class ApiErrorFactoryTests
{
    [Fact]
    public void ValidationFailed_ShouldReturnCamelCaseDetailKeys()
    {
        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Rate", "Rate must be positive.")
        ]);

        var result = ApiErrorFactory.ValidationFailed(validationResult);

        result.Details.Keys.Should().ContainSingle()
            .Which.Should().Be("rate");
    }

    [Fact]
    public void FromDomainError_ShouldReturnCamelCaseDetailKeys()
    {
        var error = DomainError.Validation(
            code: "Transaction.TransferRateMustBePositive",
            message: "Rate must be positive.",
            field: "Rate");

        var result = ApiErrorFactory.FromDomainError(error);

        result.Details.Keys.Should().ContainSingle()
            .Which.Should().Be("rate");
    }
}
