namespace MSaver.UnitTests.Features.Accounts.Create;

public sealed class CreateAccountRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenInitialBalanceIsNegative()
    {
        var sut = new CreateAccountRequestValidator();
        var request = RequestFactory.CreateAccountRequest(initialBalance: -1m);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(CreateAccountRequest.InitialBalance) &&
            x.ErrorMessage == MSaver.Application.Common.Validation.ValidationMessages.MustBeZeroOrPositive);
    }
}
