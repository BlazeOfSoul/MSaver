public sealed class GetMonthBalanceResponse
{
    public Guid AccountId { get; init; }
    public string AccountName { get; init; } = default!;
    public string CurrencyCode { get; init; } = default!;

    public decimal OpeningBalance { get; init; }
    public decimal MonthChange { get; init; }
    public decimal ClosingBalance { get; init; }

    public int Year { get; init; }
    public int Month { get; init; }
}