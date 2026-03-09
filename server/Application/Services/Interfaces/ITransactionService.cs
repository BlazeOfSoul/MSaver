using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;

namespace server.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<Guid> CreateAsync(CreateTransactionCommand command, CancellationToken cancellationToken = default);
    Task<StatisticsResponse> GetStatisticsAsync(GetStatisticsQuery query, CancellationToken cancellationToken = default);
}
