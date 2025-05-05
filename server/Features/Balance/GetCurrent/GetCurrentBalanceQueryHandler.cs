using MediatR;
using server.Models.Constants;
using server.Repositories.Interfaces;

namespace server.Features.Balance.GetCurrent;

public class GetCurrentBalanceQueryHandler : IRequestHandler<GetCurrentBalanceQuery, GetCurrentBalanceResponse>
{
    private readonly IBalanceRepository _balanceRepository;

    public GetCurrentBalanceQueryHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<GetCurrentBalanceResponse> Handle(GetCurrentBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepository.GetCurrentByUserIdAsync(request.UserId);
        if (balance == null)
            throw new Exception(ErrorMessages.Balance.NotFound);

        return new GetCurrentBalanceResponse(balance.IncomeTotal, balance.ExpenseTotal, balance.ValueTotal);
    }
}
