using server.Application.Features.Balance.GetCurrent;

namespace server.Application.Services.Interfaces;

public interface IBalanceService
{
    Task<GetCurrentBalanceResponse> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default);
}