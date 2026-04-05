namespace MSaver.Application.Features.Accounts.Get;

public sealed class GetAccountsResponse
{
    public IReadOnlyCollection<AccountItemResponse> Items { get; init; } = [];
}