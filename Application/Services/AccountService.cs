using MSaver.Api.Contracts.Accounts;
using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetById;
using MSaver.Application.Features.Accounts.GetMonthBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Application.Services;

public sealed class AccountService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<Guid>> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var existsByName = await _accountRepository.ExistsByNameAsync(
            userId,
            request.Name,
            cancellationToken);

        if (existsByName)
            return Result<Guid>.Failure(AccountDomainErrors.NameAlreadyExists);

        var hasAccounts = await _accountRepository.AnyAsync(userId, cancellationToken);
        var isPrimary = !hasAccounts;

        Account account = Account.Create(
            userId: userId,
            currencyCode: request.CurrencyCode,
            name: request.Name,
            color: request.Color,
            isPrimary: isPrimary);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(account.Id);
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null || account.UserId != userId)
            return Result<Guid>.Failure(AccountDomainErrors.NotFound);

        var exists = await _accountRepository.ExistsByNameAsync(
            userId,
            request.Name,
            cancellationToken,
            request.Id);

        if (exists)
            return Result<Guid>.Failure(AccountDomainErrors.NameAlreadyExists);

        account.Update(
            request.Name,
            request.Color);

        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(account.Id);
    }

    public async Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null || account.UserId != userId)
            return Result<Guid>.Failure(AccountDomainErrors.NotFound);

        account.Archive();

        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(account.Id);
    }

    public async Task<Result<GetAccountsResponse>> GetAccountsAsync(
        GetAccountsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var query = new AccountListQuery
        {
            UserId = userId,
            Search = ListQueryHelper.NormalizeSearch(request.Search),
            SortBy = ListQueryHelper.NormalizeSortBy(request.SortBy, AccountSortFields.CreatedAt),
            SortDirection = ListQueryHelper.NormalizeSortDirection(request.SortDirection),
            IsArchived = request.IsArchived,
            CurrencyCode = ListQueryHelper.NormalizeUpperInvariant(request.CurrencyCode),
            Page = request.Page,
            Size = request.Size
        };

        var pagedAccounts = await _accountRepository.GetPagedAsync(query, cancellationToken);
        var accountIds = pagedAccounts.Items.Select(x => x.Id).ToArray();

        var totalsByAccountId = await _transactionRepository.SumByAccountIdsAsync(
            accountIds,
            cancellationToken);

        var items = pagedAccounts.Items
            .Select(account => new AccountItemResponse
            {
                Id = account.Id,
                Name = account.Name,
                CurrencyCode = account.CurrencyCode,
                CurrentBalance = totalsByAccountId.GetValueOrDefault(account.Id, 0m),
                Color = account.Color,
                IsArchived = account.IsArchived
            })
            .ToArray();

        return Result<GetAccountsResponse>.Success(new GetAccountsResponse
        {
            Items = items,
            Page = pagedAccounts.Page,
            Size = pagedAccounts.Size,
            TotalCount = pagedAccounts.TotalCount,
            TotalPages = pagedAccounts.TotalPages,
            HasPreviousPage = pagedAccounts.HasPreviousPage,
            HasNextPage = pagedAccounts.HasNextPage
        });
    }

    public async Task<Result<GetAccountByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null || account.UserId != userId)
            return Result<GetAccountByIdResponse>.Failure(AccountDomainErrors.NotFound);

        var totalsByAccountId = await _transactionRepository.SumByAccountIdsAsync(
            [account.Id],
            cancellationToken);

        var currentBalance = totalsByAccountId.GetValueOrDefault(account.Id, 0m);

        return Result<GetAccountByIdResponse>.Success(new GetAccountByIdResponse
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            CurrentBalance = currentBalance,
            Color = account.Color,
            IsArchived = account.IsArchived,
            IsPrimary = account.IsPrimary
        });
    }

    public async Task<Result<GetMonthBalanceResponse>> GetMonthBalanceAsync(
        GetMonthBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null || account.UserId != userId)
            return Result<GetMonthBalanceResponse>.Failure(AccountDomainErrors.NotFound);

        var monthStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var beforeTotal = await _transactionRepository.GetBalanceBeforeAsync(
            account.Id,
            monthStart,
            cancellationToken);

        var monthChange = await _transactionRepository.GetBalanceInPeriodAsync(
            account.Id,
            monthStart,
            monthEnd,
            cancellationToken);

        var closingBalance = beforeTotal + monthChange;

        return Result<GetMonthBalanceResponse>.Success(new GetMonthBalanceResponse
        {
            AccountId = account.Id,
            AccountName = account.Name,
            CurrencyCode = account.CurrencyCode,
            OpeningBalance = beforeTotal,
            MonthChange = monthChange,
            ClosingBalance = closingBalance,
            Year = request.Year,
            Month = request.Month
        });
    }
}