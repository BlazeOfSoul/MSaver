namespace MSaver.Application.Features.Accounts.Get;

public static class AccountSortFields
{
    public const string CreatedAt = "createdAt";
    public const string Name = "name";
    public const string CurrencyCode = "currencyCode";

    public static readonly string[] All =
    [
        CreatedAt,
        Name,
        CurrencyCode
    ];
}