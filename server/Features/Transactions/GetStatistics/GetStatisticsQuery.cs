using MediatR;

namespace server.Features.Transactions.GetStatistics;

public class GetStatisticsQuery : IRequest<StatisticsResponse>
{
    public Guid UserId { get; set; }

    public GetStatisticsQuery(Guid userId)
    {
        UserId = userId;
    }
}