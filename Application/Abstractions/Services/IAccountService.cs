using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.Delete;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Application.Abstractions.Services;

public interface IAccountService
{
    Task<Result<CreateAccountResponse>> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        DeleteAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetAccountsResponse>> GetAccountsAsync(
        GetAccountsRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetAccountBalanceResponse>> GetBalanceAsync(
        GetAccountBalanceRequest request,
        CancellationToken cancellationToken = default);
}