using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.GetById;
using MSaver.Application.Features.Transactions.Transfer;
using MSaver.Application.Features.Transactions.Update;

namespace MSaver.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<Result<GetTransactionsResponse>> GetAsync(
        GetTransactionsRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetTransactionByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<GetTransferRateResponse>> GetTransferRateAsync(
        Guid fromAccountId,
        Guid toAccountId,
        CancellationToken cancellationToken = default);

    Task<Result<CreateTransferResponse>> TransferAsync(
        CreateTransferRequest request,
        CancellationToken cancellationToken = default);
}
