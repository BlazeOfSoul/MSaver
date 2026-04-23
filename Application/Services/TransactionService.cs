using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Common.Models;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.GetById;
using MSaver.Application.Features.Transactions.Transfer;
using MSaver.Application.Features.Transactions.Update;
using MSaver.Domain.Constants;
using MSaver.Domain.Enums;

namespace MSaver.Application.Services;

public sealed class TransactionService(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IExchangeRateService exchangeRateService) : ITransactionService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;

    public async Task<Result<GetTransactionsResponse>> GetAsync(
        GetTransactionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var query = new TransactionListQuery
        {
            UserId = userId,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Search = ListQueryHelper.NormalizeSearch(request.Search),
            SortBy = ListQueryHelper.NormalizeSortBy(request.SortBy, TransactionSortFields.Date),
            SortDirection = ListQueryHelper.NormalizeSortDirection(request.SortDirection),
            Page = request.Page,
            Size = request.Size
        };

        var pagedTransactions = await _transactionRepository.GetPagedWithDetailsAsync(query, cancellationToken);

        var items = pagedTransactions.Items
            .Select(x => new TransactionItemResponse
            {
                Id = x.Id,
                Account = new TransactionAccountResponse
                {
                    Id = x.AccountId,
                    Name = x.Account!.Name,
                    Color = x.Account.Color,
                    CurrencyCode = x.Account.CurrencyCode,
                    IsArchived = x.Account.IsArchived
                },
                Category = new TransactionCategoryResponse
                {
                    Id = x.CategoryId,
                    Name = x.Category!.Name,
                    Color = x.Category.Color
                },
                Amount = x.Amount,
                Date = x.Date,
                Description = x.Description
            })
            .ToArray();

        return Result<GetTransactionsResponse>.Success(new GetTransactionsResponse
        {
            Items = items,
            Page = pagedTransactions.Page,
            Size = pagedTransactions.Size,
            TotalCount = pagedTransactions.TotalCount,
            TotalPages = pagedTransactions.TotalPages,
            HasPreviousPage = pagedTransactions.HasPreviousPage,
            HasNextPage = pagedTransactions.HasNextPage
        });
    }

    public async Task<Result<GetTransactionByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var transaction = await _transactionRepository.GetByIdWithDetailsAsync(id, cancellationToken);

        if (transaction is null || transaction.UserId != userId)
            return Result<GetTransactionByIdResponse>.Failure(TransactionDomainErrors.TransactionNotFound);

        var response = new GetTransactionByIdResponse
        {
            Id = transaction.Id,
            Account = new TransactionAccountResponse
            {
                Id = transaction.AccountId,
                Name = transaction.Account!.Name,
                Color = transaction.Account.Color,
                CurrencyCode = transaction.Account.CurrencyCode,
                IsArchived = transaction.Account.IsArchived
            },
            Category = new TransactionCategoryResponse
            {
                Id = transaction.CategoryId,
                Name = transaction.Category!.Name,
                Color = transaction.Category.Color
            },
            Amount = transaction.Amount,
            Date = transaction.Date,
            Description = transaction.Description
        };

        return Result<GetTransactionByIdResponse>.Success(response);
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var validation = await ValidateTransactionRequestAsync(
            userId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (validation.IsFailure)
            return Result<Guid>.Failure(validation.Error);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null || account.UserId != userId)
            return Result<Guid>.Failure(AccountDomainErrors.NotFound);

        Transaction transaction = Transaction.Create(
            userId: userId,
            accountId: request.AccountId,
            categoryId: request.CategoryId,
            amount: request.Amount,
            date: request.Date,
            description: request.Description);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transaction.Id);
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var transaction = await _transactionRepository.GetByIdWithCategoryAsync(request.Id, cancellationToken);
        if (transaction is null || transaction.UserId != userId)
            return Result<Guid>.Failure(TransactionDomainErrors.TransactionNotFound);

        var validation = await ValidateTransactionRequestAsync(
            userId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (validation.IsFailure)
            return Result<Guid>.Failure(validation.Error);

        transaction.Update(
            categoryId: request.CategoryId,
            amount: request.Amount,
            date: request.Date,
            description: request.Description);

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transaction.Id);
    }

    public async Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction is null || transaction.UserId != userId)
            return Result<Guid>.Failure(TransactionDomainErrors.TransactionNotFound);

        await _transactionRepository.RemoveAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transaction.Id);
    }

    public async Task<Result<CreateTransferResponse>> TransferAsync(
        CreateTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId, cancellationToken);
        if (fromAccount is null || fromAccount.UserId != userId || fromAccount.IsArchived)
            return Result<CreateTransferResponse>.Failure(AccountDomainErrors.NotFound);

        var toAccount = await _accountRepository.GetByIdAsync(request.ToAccountId, cancellationToken);
        if (toAccount is null || toAccount.UserId != userId || toAccount.IsArchived)
            return Result<CreateTransferResponse>.Failure(AccountDomainErrors.NotFound);

        if (fromAccount.Id == toAccount.Id)
            return Result<CreateTransferResponse>.Failure(TransactionDomainErrors.TransferAccountsMustBeDifferent);

        if (request.Amount <= 0)
            return Result<CreateTransferResponse>.Failure(TransactionDomainErrors.TransferAmountMustBeGreaterThanZero);

        decimal rate;

        if (request.Rate.HasValue)
        {
            if (request.Rate.Value <= 0)
                return Result<CreateTransferResponse>.Failure(TransactionDomainErrors.TransferRateMustBePositive);

            rate = request.Rate.Value;
        }
        else
        {
            rate = fromAccount.CurrencyCode == toAccount.CurrencyCode
                ? 1m
                : await _exchangeRateService.GetRateAsync(
                    fromAccount.CurrencyCode,
                    toAccount.CurrencyCode,
                    cancellationToken);
        }

        var precision = CurrencyDefinitions.Get(toAccount.CurrencyCode).Precision;

        var depositAmount = Math.Round(
            request.Amount * rate,
            precision,
            MidpointRounding.AwayFromZero);

        var transferExpenseCategory = await _categoryRepository.GetTransferExpenseCategoryAsync(userId, cancellationToken);
        if (transferExpenseCategory is null)
            return Result<CreateTransferResponse>.Failure(CategoryDomainErrors.NotFound);

        var transferIncomeCategory = await _categoryRepository.GetTransferIncomeCategoryAsync(userId, cancellationToken);
        if (transferIncomeCategory is null)
            return Result<CreateTransferResponse>.Failure(CategoryDomainErrors.NotFound);

        var expenseTransaction = Transaction.Create(
            userId: userId,
            accountId: fromAccount.Id,
            categoryId: transferExpenseCategory.Id,
            amount: request.Amount,
            date: request.Date,
            description: request.Description);

        var incomeTransaction = Transaction.Create(
            userId: userId,
            accountId: toAccount.Id,
            categoryId: transferIncomeCategory.Id,
            amount: depositAmount,
            date: request.Date,
            description: request.Description);

        await _transactionRepository.AddAsync(expenseTransaction, cancellationToken);
        await _transactionRepository.AddAsync(incomeTransaction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateTransferResponse>.Success(
            new CreateTransferResponse(
                ExpenseTransactionId: expenseTransaction.Id,
                IncomeTransactionId: incomeTransaction.Id,
                WithdrawAmount: request.Amount,
                DepositAmount: depositAmount,
                Rate: rate,
                FromCurrencyCode: fromAccount.CurrencyCode,
                ToCurrencyCode: toAccount.CurrencyCode));
    }

    private async Task<(bool IsFailure, DomainError Error, Category? Category)> ValidateTransactionRequestAsync(
        Guid userId,
        Guid categoryId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return (true, UserDomainErrors.UserNotFound, null);

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null || category.UserId != userId)
            return (true, TransactionDomainErrors.CategoryNotFound, null);

        if (category.IsDeleted)
            return (true, CategoryDomainErrors.CategoryDeleted, null);

        if (amount == 0)
            return (true, TransactionDomainErrors.AmountMustNotBeZero, null);

        if (category.Type == CategoryType.Debit && amount > 0 ||
            category.Type == CategoryType.Credit && amount < 0)
        {
            return (true, TransactionDomainErrors.AmountSignMismatchWithCategoryType, null);
        }

        return (false, null!, category);
    }
}