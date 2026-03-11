using server.Application.Common.Results;
using server.Application.Features.Balance.GetCurrent;

namespace server.Application.Services.Interfaces;

public interface IBalanceService
{
    Task<Result<GetCurrentBalanceResponse>> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
