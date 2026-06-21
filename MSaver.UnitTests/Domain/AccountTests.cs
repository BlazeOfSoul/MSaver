namespace MSaver.UnitTests.Domain;

public sealed class AccountTests
{
    [Theory]
    [InlineData("AMD")]
    [InlineData("ARS")]
    [InlineData("AZN")]
    [InlineData("BRL")]
    [InlineData("BYN")]
    [InlineData("CHF")]
    [InlineData("CLP")]
    [InlineData("CNY")]
    [InlineData("COP")]
    [InlineData("CZK")]
    [InlineData("DKK")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("GEL")]
    [InlineData("HKD")]
    [InlineData("INR")]
    [InlineData("JPY")]
    [InlineData("KRW")]
    [InlineData("KZT")]
    [InlineData("NOK")]
    [InlineData("PEN")]
    [InlineData("PLN")]
    [InlineData("RUB")]
    [InlineData("SEK")]
    [InlineData("SGD")]
    [InlineData("THB")]
    [InlineData("UAH")]
    [InlineData("USD")]
    [InlineData("UYU")]
    public void Create_ShouldAcceptEveryCurrencyExposedByTheApp(string currencyCode)
    {
        var account = Account.Create(
            AccountTestData.UserId,
            currencyCode.ToLowerInvariant(),
            "Main account");

        account.CurrencyCode.Should().Be(currencyCode);
    }
}
