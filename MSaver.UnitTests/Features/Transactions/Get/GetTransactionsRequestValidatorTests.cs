using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Common.Validation;
using MSaver.Application.Features.Transactions.Get;

namespace MSaver.UnitTests.Features.Transactions.Get;

public sealed class GetTransactionsRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldAllowWhitespaceAroundSortFields()
    {
        var sut = new GetTransactionsRequestValidator();
        var request = new GetTransactionsRequest
        {
            SortBy = $"  {TransactionSortFields.Amount}  ",
            SortDirection = "  ASC  "
        };

        var result = sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnToDateError_WhenFromDateIsAfterToDate()
    {
        var sut = new GetTransactionsRequestValidator();
        var request = new GetTransactionsRequest
        {
            FromDate = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(GetTransactionsRequest.ToDate) &&
            x.ErrorMessage == ListValidationMessages.InvalidDateRange);
    }

    [Fact]
    public void Validate_ShouldAllowSameDayRange_WhenToDateIsDateOnly()
    {
        var sut = new GetTransactionsRequestValidator();
        var request = new GetTransactionsRequest
        {
            FromDate = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
