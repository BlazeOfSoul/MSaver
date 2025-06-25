using MediatR;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.Enums;

namespace server.Features.Transactions.GetStatistics;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, StatisticsResponse>
{
    private readonly ApplicationDbContext _context;

    public GetStatisticsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StatisticsResponse> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var response = new StatisticsResponse();
        var yearsSet = new HashSet<int>();
        var monthsByYear = new Dictionary<int, HashSet<int>>();

        foreach (var t in transactions)
        {
            var year = t.Date.Year;
            var month = t.Date.Month - 1;

            yearsSet.Add(year);
            if (!monthsByYear.ContainsKey(year))
                monthsByYear[year] = new HashSet<int>();
            monthsByYear[year].Add(month);

            var isIncome = t.Category.Type == CategoryType.Income;
            var targetChart = isIncome ? response.IncomeChartDataByYear : response.ExpenseChartDataByYear;
            var targetTable = isIncome ? response.IncomeTableData : response.ExpenseTableData;

            if (!targetChart.ContainsKey(year))
                targetChart[year] = new List<ChartDataDto>(new ChartDataDto[12]);

            if (targetChart[year][month] == null)
                targetChart[year][month] = new ChartDataDto();

            var chartMonth = targetChart[year][month];
            var categoryIndex = chartMonth.Labels.IndexOf(t.Category.Name);
            if (categoryIndex == -1)
            {
                chartMonth.Labels.Add(t.Category.Name);
                chartMonth.Data.Add(t.Amount);
                chartMonth.BackgroundColors.Add(t.Category.Color); 
            }
            else
            {
                chartMonth.Data[categoryIndex] += t.Amount;
            }


            if (!targetTable.ContainsKey(year))
                targetTable[year] = new();

            if (!targetTable[year].ContainsKey(t.Category.Name))
                targetTable[year][t.Category.Name] = new();

            if (!targetTable[year][t.Category.Name].ContainsKey(month))
                targetTable[year][t.Category.Name][month] = 0;

            targetTable[year][t.Category.Name][month] += t.Amount;
        }

        response.AvailableYears = yearsSet.OrderBy(y => y).ToList();
        response.AvailableMonthsByYear = monthsByYear.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(m => m).ToList()
        );

        return response;
    }
}
