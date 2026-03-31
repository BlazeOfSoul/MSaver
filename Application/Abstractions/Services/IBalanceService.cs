using MSaver.Application.Common.Results;
using MSaver.Application.Features.Balance.GetCurrent;

namespace MSaver.Application.Abstractions.Services;

public interface IBalanceService
{
    Task<Result<GetCurrentBalanceResponse>> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
