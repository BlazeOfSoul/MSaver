using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.Update;
using MSaver.Domain.Enums;

namespace MSaver.Application.Services;

public sealed class TransactionService(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ITransactionService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<Guid>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var validation = await ValidateTransactionRequestAsync(
            userId,
            request.AccountId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (validation.IsFailure)
            return Result<Guid>.Failure(validation.Error);

        var account = validation.Account!;

        Transaction transaction = Transaction.Create(
            userId: userId,
            accountId: request.AccountId,
            categoryId: request.CategoryId,
            currencyId: account.CurrencyId,
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
            request.AccountId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (validation.IsFailure)
            return Result<Guid>.Failure(validation.Error);

        var account = validation.Account!;

        transaction.Update(
            categoryId: request.CategoryId,
            amount: request.Amount,
            date: request.Date,
            description: request.Description);

        if (transaction.AccountId != request.AccountId ||
            transaction.CurrencyId != account.CurrencyId)
        {
            transaction.ChangeAccount(request.AccountId, account.CurrencyId);
        }

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

    public async Task<Result<GetTransactionsResponse>> GetByUserAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var transactions = await _transactionRepository
            .GetByUserIdWithDetailsAsync(userId, cancellationToken);

        var items = transactions
            .OrderByDescending(x => x.Date)
            .Select(x => new TransactionItemResponse
            {
                Id = x.Id,
                Account = new TransactionAccountResponse
                {
                    Id = x.AccountId,
                    Name = x.Account!.Name,
                    Color = x.Account.Color,
                    CurrencyId = x.Account.CurrencyId,
                    CurrencyCode = x.Account.Currency!.Code,
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
            Items = items
        });
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