namespace MSaver.UnitTests.Common.TestData;

public static class RequestFactory
{
    public static CreateAccountRequest CreateAccountRequest(
        string name = "Main account",
        string currencyCode = "USD",
        string? color = "#111111")
    {
        return new CreateAccountRequest
        {
            Name = name,
            CurrencyCode = currencyCode,
            Color = color
        };
    }

    public static UpdateAccountRequest UpdateAccountRequest(
        Guid? id = null,
        string name = "Updated account",
        string? color = "#222222")
    {
        return new UpdateAccountRequest(
            id ?? Guid.NewGuid(),
            name,
            color);
    }

    public static GetMonthBalanceRequest GetMonthBalanceRequest(
        Guid? accountId = null,
        int year = 2026,
        int month = 5)
    {
        return new GetMonthBalanceRequest(
            accountId ?? Guid.NewGuid(),
            year,
            month);
    }
}