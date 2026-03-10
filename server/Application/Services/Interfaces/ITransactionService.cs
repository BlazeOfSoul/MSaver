using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;

namespace server.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<Guid> CreateAsync(CreateTransactionRequest command, CancellationToken cancellationToken = default);
    Task<StatisticsResponse> GetStatisticsAsync(GetStatisticsRequest query, CancellationToken cancellationToken = default);
}
