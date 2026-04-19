using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.Get;
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

    public async Task<Result<GetAccountsResponse>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var accounts = await _accountRepository.GetAsync(userId, cancellationToken);
        var accountIds = accounts.Select(x => x.Id).ToArray();

        var totalsByAccountId = await _transactionRepository.SumByAccountIdsAsync(
            accountIds,
            cancellationToken);

        var items = accounts
            .Select(account =>
            {
                var total = totalsByAccountId.GetValueOrDefault(account.Id, 0m);

                return new AccountItemResponse
                {
                    Id = account.Id,
                    Name = account.Name,
                    CurrencyCode = account.CurrencyCode,
                    CurrentBalance = total,
                    Color = account.Color,
                    IsArchived = account.IsArchived
                };
            })
            .ToArray();

        return Result<GetAccountsResponse>.Success(new GetAccountsResponse
        {
            Items = items
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

        var monthStart = new DateTime(request.Year, request.Month, 1);
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