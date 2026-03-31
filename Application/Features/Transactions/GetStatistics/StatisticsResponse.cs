namespace MSaver.Application.Features.Transactions.GetStatistics;

public sealed class StatisticsResponse
{
    public Dictionary<int, List<ChartDataItem>> IncomeChartDataByYear { get; set; } = new();
    public Dictionary<int, List<ChartDataItem>> ExpenseChartDataByYear { get; set; } = new();

    public Dictionary<int, Dictionary<string, Dictionary<int, decimal>>> IncomeTableData { get; set; } = new();
    public Dictionary<int, Dictionary<string, Dictionary<int, decimal>>> ExpenseTableData { get; set; } = new();

    public List<int> AvailableYears { get; set; } = new();
    public Dictionary<int, List<int>> AvailableMonthsByYear { get; set; } = new();
}