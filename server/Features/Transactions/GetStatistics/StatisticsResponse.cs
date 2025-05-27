namespace server.Features.Transactions.GetStatistics;

public class StatisticsResponse
{
    public Dictionary<int, List<ChartDataDto>> IncomeChartDataByYear { get; set; } = new();
    public Dictionary<int, List<ChartDataDto>> ExpenseChartDataByYear { get; set; } = new();

    public Dictionary<int, Dictionary<string, decimal[]>> IncomeTableData { get; set; } = new();
    public Dictionary<int, Dictionary<string, decimal[]>> ExpenseTableData { get; set; } = new();

    public List<int> AvailableYears { get; set; } = new();
    public Dictionary<int, List<int>> AvailableMonthsByYear { get; set; } = new();
}