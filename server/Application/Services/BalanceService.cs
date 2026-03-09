using server.Application.Constants;
using server.Application.Features.Balance.GetCurrent;
using server.Application.Services.Interfaces;
using server.Application.Abstractions.Repositories;

namespace server.Application.Services;

public sealed class BalanceService : IBalanceService
{
    private readonly IBalanceRepository _balanceRepository;

    public BalanceService(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<GetCurrentBalanceResponse> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetCurrentByUserIdAsync(userId, cancellationToken);

        if (balance is null)
        {
            throw new InvalidOperationException(ErrorMessages.Balance.NotFound);
        }

        return new GetCurrentBalanceResponse(
            balance.IncomeTotal,
            balance.ExpenseTotal,
            balance.ValueTotal);
    }
}
