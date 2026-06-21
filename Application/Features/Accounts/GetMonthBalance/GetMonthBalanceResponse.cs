namespace MSaver.Application.Features.Accounts.GetMonthBalance;

public sealed class GetMonthBalanceResponse
{
    public Guid AccountId { get; init; }
    public string AccountName { get; init; } = default!;
    public string CurrencyCode { get; init; } = default!;

    public decimal OpeningBalance { get; init; }
    public decimal MonthChange { get; init; }
    public decimal ClosingBalance { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal TransferIn { get; init; }
    public decimal TransferOut { get; init; }
    public decimal OperationsChange { get; init; }
    public decimal TransferChange { get; init; }

    public int Year { get; init; }
    public int Month { get; init; }
}
