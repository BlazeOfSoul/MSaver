using MSaver.Api.Contracts.Accounts;
using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetById;
using MSaver.Application.Features.Accounts.GetMonthBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Application.Abstractions.Services;

public interface IAccountService
{
    Task<Result<Guid>> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<GetAccountsResponse>> GetAccountsAsync(
        GetAccountsRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetAccountByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<GetMonthBalanceResponse>> GetMonthBalanceAsync(
        GetMonthBalanceRequest request,
        CancellationToken cancellationToken = default);
}