using MSaver.Application.Common.Results;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.GetStatistics;

namespace MSaver.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest command,
        CancellationToken cancellationToken = default);

    Task<Result<StatisticsResponse>> GetStatisticsAsync(
        GetStatisticsRequest query,
        CancellationToken cancellationToken = default);
}
