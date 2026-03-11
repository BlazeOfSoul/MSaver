using Microsoft.EntityFrameworkCore;
using server.Application.Common.Results;
using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;
using server.Application.Services.Interfaces;
using server.Domain.Common;
using server.Domain.Entities;
using server.Domain.Enums;
using server.Domain.Errors;
using server.Infrastructure.Persistence;

namespace server.Application.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(
                c => c.Id == request.CategoryId && c.UserId == request.UserId,
                cancellationToken);

        if (category is null)
        {
            return Result<Guid>.Failure(TransactionDomainErrors.CategoryNotFound);
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
            balance = Balance.Create(
                request.UserId,
                now.Year,
                now.Month);

            _dbContext.Balances.Add(balance);
        }

        try
        {
            var transaction = Transaction.Create(
                request.UserId,
                request.CategoryId,
                request.Amount,
                request.Date,
                request.Description);

            _dbContext.Transactions.Add(transaction);

            if (category.Type == CategoryType.Income)
            {
                balance.ApplyIncome(request.Amount);
            }
            else
            {
                balance.ApplyExpense(request.Amount);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(transaction.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<StatisticsResponse>> GetStatisticsAsync(
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
            var month = t.Date.Month - 1;

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
                targetChart[year] = Enumerable.Range(0, 12)
                    .Select(_ => new ChartDataDto())
                    .ToList();
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

        return Result<StatisticsResponse>.Success(response);
    }
}
