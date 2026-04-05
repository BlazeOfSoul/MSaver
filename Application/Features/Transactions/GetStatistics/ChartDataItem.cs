namespace MSaver.Application.Features.Transactions.GetStatistics;

public sealed class ChartDataItem
{
    public List<string> Labels { get; set; } = [];
    public List<decimal> Data { get; set; } = [];
    public List<string> BackgroundColors { get; set; } = [];
}