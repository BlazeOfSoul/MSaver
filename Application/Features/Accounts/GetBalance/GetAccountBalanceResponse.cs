namespace MSaver.Application.Features.Accounts.GetBalance;

public sealed class GetAccountBalanceResponse
{
    public Guid AccountId { get; init; }

    public string AccountName { get; init; } = string.Empty;

    public decimal InitialBalance { get; init; }

    public decimal CurrentBalance { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;
}