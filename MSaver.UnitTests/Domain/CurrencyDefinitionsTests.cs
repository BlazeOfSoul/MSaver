using MSaver.Domain.Common;
using MSaver.Domain.Constants;

namespace MSaver.UnitTests.Domain;

public sealed class CurrencyDefinitionsTests
{
    [Theory]
    [InlineData("BYN")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("RUB")]
    [InlineData("GBP")]
    [InlineData("CHF")]
    [InlineData("PLN")]
    [InlineData("CZK")]
    [InlineData("SEK")]
    [InlineData("NOK")]
    [InlineData("DKK")]
    [InlineData("CNY")]
    [InlineData("JPY")]
    [InlineData("KRW")]
    [InlineData("INR")]
    [InlineData("SGD")]
    [InlineData("HKD")]
    [InlineData("THB")]
    [InlineData("KZT")]
    [InlineData("UAH")]
    [InlineData("GEL")]
    [InlineData("AMD")]
    [InlineData("AZN")]
    [InlineData("BRL")]
    [InlineData("ARS")]
    [InlineData("CLP")]
    [InlineData("COP")]
    [InlineData("PEN")]
    [InlineData("UYU")]
    public void Exists_ShouldReturnTrue_ForSupportedCurrency(string code)
    {
        CurrencyDefinitions.Exists(code).Should().BeTrue();
    }

    [Fact]
    public void Get_ShouldNormalizeCurrencyCode()
    {
        CurrencyDefinitions.Get(" eur ").Code.Should().Be("EUR");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Get_ShouldThrowDomainException_WhenCurrencyCodeIsMissing(string? code)
    {
        var action = () => CurrencyDefinitions.Get(code!);

        action.Should()
            .Throw<DomainException>()
            .Which.Error.Should().Be(AccountDomainErrors.CurrencyNotFound);
    }
}
