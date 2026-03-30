namespace server.Application.Features.Transactions.GetStatistics;

public sealed class ChartDataItem
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Data { get; set; } = new();
    public List<string> BackgroundColors { get; set; } = new();
}