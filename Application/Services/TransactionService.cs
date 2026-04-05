using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Delete;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.GetStatistics;
using MSaver.Application.Features.Transactions.Update;
using MSaver.Domain.Enums;

namespace MSaver.Application.Services;

public sealed class TransactionService(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork) : ITransactionService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateTransactionRequestAsync(
            request.UserId,
            request.AccountId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (validation.IsFailure)
            return Result<Guid>.Failure(validation.Error);

        var account = validation.Account!;

        try
        {
            var transaction = Transaction.Create(
                userId: request.UserId,
                accountId: request.AccountId,
                categoryId: request.CategoryId,
                currencyId: account.CurrencyId,
                amount: request.Amount,
                date: request.Date,
                description: request.Description,
                baseCurrencyId: null,
                amountBase: null);

            if (request.TagIds is not null && request.TagIds.Count > 0)
            {
                var distinctTagIds = request.TagIds.Distinct().ToArray();

                foreach (var tagId in distinctTagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
                    if (tag is null || tag.UserId != request.UserId || tag.IsDeleted)
                        return Result<Guid>.Failure(TagDomainErrors.TagNotFound);
                }

                transaction.ReplaceTags(distinctTagIds);
            }

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(transaction.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdWithCategoryAsync(request.Id, cancellationToken);
        if (transaction is null || transaction.UserId != request.UserId)
            return Result<Guid>.Failure(TransactionDomainErrors.TransactionNotFound);

        if (transaction.IsTransfer())
            return Result<Guid>.Failure(TransactionDomainErrors.TransferTransactionCannotBeEditedAsRegular);

        var validation = await ValidateTransactionRequestAsync(
            request.UserId,
            request.AccountId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (validation.IsFailure)
            return Result<Guid>.Failure(validation.Error);

        var account = validation.Account!;

        try
        {
            transaction.Update(
                categoryId: request.CategoryId,
                amount: request.Amount,
                date: request.Date,
                description: request.Description,
                baseCurrencyId: null,
                amountBase: null);

            if (transaction.AccountId != request.AccountId || transaction.CurrencyId != account.CurrencyId)
            {
                transaction.ChangeAccount(request.AccountId, account.CurrencyId);
            }

            var tagIds = request.TagIds?.Distinct().ToArray() ?? Array.Empty<Guid>();

            if (tagIds.Length > 0)
            {
                foreach (var tagId in tagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
                    if (tag is null || tag.UserId != request.UserId || tag.IsDeleted)
                        return Result<Guid>.Failure(TagDomainErrors.TagNotFound);
                }
            }

            transaction.ReplaceTags(tagIds);

            await _transactionRepository.UpdateAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(transaction.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> DeleteAsync(
        DeleteTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null || transaction.UserId != request.UserId)
            return Result<Guid>.Failure(TransactionDomainErrors.TransactionNotFound);

        if (transaction.IsTransfer())
            return Result<Guid>.Failure(TransactionDomainErrors.TransferTransactionCannotBeDeletedAsRegular);

        await _transactionRepository.RemoveAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transaction.Id);
    }

    public async Task<Result<GetTransactionsResponse>> GetByUserAsync(
        GetTransactionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository
            .GetByUserIdWithCategoryAsync(request.UserId, cancellationToken);

        var items = transactions
            .OrderByDescending(x => x.Date)
            .Select(x => new TransactionItemResponse
            {
                Id = x.Id,
                AccountId = x.AccountId,
                CategoryId = x.CategoryId,
                CategoryName = x.Category!.Name,
                CategoryColor = x.Category.Color,
                Amount = x.Amount,
                Date = x.Date,
                Description = x.Description,
                TagIds = x.TransactionTags.Select(tt => tt.TagId).ToArray(),
                Tags = x.TransactionTags.Select(tt => tt.Tag!.Name).ToArray(),
                IsTransfer = x.TransferId.HasValue
            })
            .ToArray();

        return Result<GetTransactionsResponse>.Success(new GetTransactionsResponse
        {
            Items = items
        });
    }

    public async Task<Result<StatisticsResponse>> GetStatisticsAsync(
        GetStatisticsRequest query,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository
            .GetByUserIdWithCategoryAsync(query.UserId, cancellationToken);

        var regularTransactions = transactions
            .Where(t => !t.TransferId.HasValue)
            .ToList();

        var response = new StatisticsResponse();
        var yearsSet = new HashSet<int>();
        var monthsByYear = new Dictionary<int, HashSet<int>>();

        foreach (var t in regularTransactions)
        {
            var year = t.Date.Year;
            var month = t.Date.Month - 1;

            yearsSet.Add(year);

            if (!monthsByYear.ContainsKey(year))
            {
                monthsByYear[year] = new HashSet<int>();
            }

            monthsByYear[year].Add(month);

            var isIncome = t.Amount > 0;
            var amountAbs = Math.Abs(t.Amount);

            var targetChart = isIncome ? response.IncomeChartDataByYear : response.ExpenseChartDataByYear;
            var targetTable = isIncome ? response.IncomeTableData : response.ExpenseTableData;

            if (!targetChart.ContainsKey(year))
            {
                targetChart[year] = Enumerable.Range(0, 12)
                    .Select(_ => new ChartDataItem())
                    .ToList();
            }

            var chartMonth = targetChart[year][month];
            var categoryIndex = chartMonth.Labels.IndexOf(t.Category!.Name);

            if (categoryIndex == -1)
            {
                chartMonth.Labels.Add(t.Category.Name);
                chartMonth.Data.Add(amountAbs);
                chartMonth.BackgroundColors.Add(t.Category.Color);
            }
            else
            {
                chartMonth.Data[categoryIndex] += amountAbs;
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

            targetTable[year][t.Category.Name][month] += amountAbs;
        }

        response.AvailableYears = yearsSet.OrderBy(y => y).ToList();
        response.AvailableMonthsByYear = monthsByYear
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(m => m).ToList());

        return Result<StatisticsResponse>.Success(response);
    }

    private async Task<(bool IsFailure, DomainError Error, Account? Account, Category? Category)> ValidateTransactionRequestAsync(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return (true, UserDomainErrors.UserNotFound, null, null);

        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account is null || account.UserId != userId)
            return (true, AccountDomainErrors.AccountNotFound, null, null);

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null || category.UserId != userId)
            return (true, TransactionDomainErrors.CategoryNotFound, null, null);

        if (category.IsDeleted)
            return (true, CategoryDomainErrors.CategoryDeleted, null, null);

        if (amount == 0)
            return (true, TransactionDomainErrors.AmountMustNotBeZero, null, null);

        if (category.Type == CategoryType.Debit && amount > 0 ||
            category.Type == CategoryType.Credit && amount < 0)
        {
            return (true, TransactionDomainErrors.AmountSignMismatchWithCategoryType, null, null);
        }

        return (false, null!, account, category);
    }
}