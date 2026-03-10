using Microsoft.EntityFrameworkCore;
using server.Application.Constants;
using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;
using server.Application.Services.Interfaces;
using server.Domain.Entities;
using server.Domain.Enums;
using server.Infrastructure.Persistence;

namespace server.Application.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(
                c => c.Id == request.CategoryId && c.UserId == request.UserId,
                cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException(ErrorMessages.Categories.NotFound);
        }

        var now = request.Date;

        var balance = await _dbContext.Balances
            .FirstOrDefaultAsync(
                mb => mb.UserId == request.UserId &&
                       mb.Year == now.Year &&
                       mb.Month == now.Month,
                cancellationToken);

        if (balance is null)
        {
            balance = new Balance
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Year = now.Year,
                Month = now.Month,
                IncomeTotal = 0,
                ExpenseTotal = 0,
                ValueTotal = 0
            };

            _dbContext.Balances.Add(balance);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            Amount = request.Amount,
            Date = request.Date
        };

        _dbContext.Transactions.Add(transaction);

        if (category.Type == CategoryType.Income)
        {
            balance.IncomeTotal += request.Amount;
        }
        else
        {
            balance.ExpenseTotal += request.Amount;
        }

        balance.ValueTotal = balance.IncomeTotal - balance.ExpenseTotal;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }

    public async Task<StatisticsResponse> GetStatisticsAsync(
        GetStatisticsRequest query,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _dbContext.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == query.UserId)
            .ToListAsync(cancellationToken);

        var response = new StatisticsResponse();
        var yearsSet = new HashSet<int>();
        var monthsByYear = new Dictionary<int, HashSet<int>>();

        foreach (var t in transactions)
        {
            var year = t.Date.Year;
            var month = t.Date.Month - 1; // 0-based for charts

            yearsSet.Add(year);
            if (!monthsByYear.ContainsKey(year))
            {
                monthsByYear[year] = new HashSet<int>();
            }
            monthsByYear[year].Add(month);

            var isIncome = t.Category.Type == CategoryType.Income;
            var targetChart = isIncome ? response.IncomeChartDataByYear : response.ExpenseChartDataByYear;
            var targetTable = isIncome ? response.IncomeTableData : response.ExpenseTableData;

            if (!targetChart.ContainsKey(year))
            {
                targetChart[year] = new List<ChartDataDto?>(new ChartDataDto?[12]).Cast<ChartDataDto>().ToList();
            }

            if (targetChart[year][month] == null)
            {
                targetChart[year][month] = new ChartDataDto();
            }

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
            {
                targetTable[year] = new();
            }

            if (!targetTable[year].ContainsKey(t.Category.Name))
            {
                targetTable[year][t.Category.Name] = new();
            }

            if (!targetTable[year][t.Category.Name].ContainsKey(month))
            {
                targetTable[year][t.Category.Name][month] = 0;
            }

            targetTable[year][t.Category.Name][month] += t.Amount;
        }

        response.AvailableYears = yearsSet.OrderBy(y => y).ToList();
        response.AvailableMonthsByYear = monthsByYear
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(m => m).ToList());

        return response;
    }
}
