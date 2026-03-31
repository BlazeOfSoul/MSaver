using MSaver.Application.Common.Results;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.GetStatistics;
using MSaver.Application.Abstractions.Services;
using MSaver.Domain.Common;
using MSaver.Domain.Entities;
using MSaver.Domain.Enums;
using MSaver.Domain.Errors;
using MSaver.Domain.Repositories;

namespace MSaver.Application.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBalanceRepository _balanceRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        ICategoryRepository categoryRepository,
        IBalanceRepository balanceRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _balanceRepository = balanceRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository
            .GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null || category.UserId != request.UserId)
        {
            return Result<Guid>.Failure(TransactionDomainErrors.CategoryNotFound);
        }

        var now = request.Date;

        var balance = await _balanceRepository
            .GetByUserAndDateAsync(request.UserId, now.Year, now.Month, cancellationToken);

        if (balance is null)
        {
            balance = Balance.Create(request.UserId, now.Year, now.Month);
            await _balanceRepository.AddAsync(balance, cancellationToken);
        }

        try
        {
            var transaction = Transaction.Create(
                request.UserId,
                request.CategoryId,
                request.Amount,
                request.Date,
                request.Description);

            await _transactionRepository.AddAsync(transaction, cancellationToken);

            if (category.Type == CategoryType.Income)
            {
                balance.ApplyIncome(request.Amount);
            }
            else
            {
                balance.ApplyExpense(request.Amount);
            }

            await _balanceRepository.UpdateAsync(balance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var transactions = await _transactionRepository
            .GetByUserIdWithCategoryAsync(query.UserId, cancellationToken);

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
                    .Select(_ => new ChartDataItem())
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
