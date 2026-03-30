using server.Application.Common.Results;
using server.Application.Features.Balance.GetCurrent;

namespace server.Application.Abstractions.Services;

public interface IBalanceService
{
    Task<Result<GetCurrentBalanceResponse>> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
