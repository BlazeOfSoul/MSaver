namespace MSaver.Application.Features.Accounts.GetMonthBalance;

public sealed record GetMonthBalanceRequest(
    Guid AccountId,
    int Year,
    int Month);