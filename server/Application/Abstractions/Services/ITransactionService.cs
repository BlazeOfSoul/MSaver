using server.Application.Common.Results;
using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;

namespace server.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest command,
        CancellationToken cancellationToken = default);

    Task<Result<StatisticsResponse>> GetStatisticsAsync(
        GetStatisticsRequest query,
        CancellationToken cancellationToken = default);
}
