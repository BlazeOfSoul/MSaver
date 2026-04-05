namespace MSaver.Application.Features.Accounts.GetBalance;

public sealed record GetAccountBalanceRequest(Guid UserId, Guid AccountId);