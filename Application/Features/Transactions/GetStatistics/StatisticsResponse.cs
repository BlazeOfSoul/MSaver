namespace MSaver.Application.Features.Transactions.GetStatistics;

public sealed class StatisticsResponse
{
    public Dictionary<int, List<ChartDataItem>> IncomeChartDataByYear { get; set; } = [];
    public Dictionary<int, List<ChartDataItem>> ExpenseChartDataByYear { get; set; } = [];

    public Dictionary<int, Dictionary<string, Dictionary<int, decimal>>> IncomeTableData { get; set; } = [];
    public Dictionary<int, Dictionary<string, Dictionary<int, decimal>>> ExpenseTableData { get; set; } = [];

    public List<int> AvailableYears { get; set; } = [];
    public Dictionary<int, List<int>> AvailableMonthsByYear { get; set; } = [];
}