namespace MSaver.Application.Features.Accounts.GetMonthBalance;

public sealed class GetMonthBalanceRequest
{
    public Guid AccountId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}