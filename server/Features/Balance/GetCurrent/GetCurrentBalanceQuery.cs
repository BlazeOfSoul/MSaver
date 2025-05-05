using MediatR;

namespace server.Features.Balance.GetCurrent;

public record GetCurrentBalanceQuery(Guid UserId) : IRequest<GetCurrentBalanceResponse>;
