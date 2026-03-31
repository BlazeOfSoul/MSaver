using MSaver.Application.Common.Results;
using MSaver.Application.Features.Balance.GetCurrent;
using MSaver.Application.Abstractions.Services;
using MSaver.Domain.Errors;
using MSaver.Domain.Repositories;

namespace MSaver.Application.Services;

public sealed class BalanceService : IBalanceService
{
    private readonly IBalanceRepository _balanceRepository;

    public BalanceService(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<Result<GetCurrentBalanceResponse>> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetCurrentByUserIdAsync(userId, cancellationToken);

        if (balance is null)
            return Result<GetCurrentBalanceResponse>.Failure(BalanceDomainErrors.NotFound);

        var response = new GetCurrentBalanceResponse(
            balance.IncomeTotal,
            balance.ExpenseTotal,
            balance.ValueTotal);

        return Result<GetCurrentBalanceResponse>.Success(response);
    }
}
