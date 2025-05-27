namespace server.Features.Transactions.GetStatistics;

public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Data { get; set; } = new();
}