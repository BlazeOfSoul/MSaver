using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Delete;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.GetStatistics;
using MSaver.Application.Features.Transactions.Update;

namespace MSaver.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        DeleteTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetTransactionsResponse>> GetByUserAsync(
        GetTransactionsRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<StatisticsResponse>> GetStatisticsAsync(
        GetStatisticsRequest request,
        CancellationToken cancellationToken = default);
}