using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.CreatePrimary;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetMonthBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Application.Services;

public sealed class AccountService(
    IAccountRepository accountRepository,
    ICurrencyRepository currencyRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICurrencyRepository _currencyRepository = currencyRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<CreateAccountResponse>> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        try
        {
            var currency = await _currencyRepository.GetByIdAsync(request.CurrencyId, cancellationToken);
            if (currency is null)
                return Result<CreateAccountResponse>.Failure(AccountDomainErrors.CurrencyNotFound);

            var exists = await _accountRepository.ExistsByNameAsync(
                userId,
                request.Name,
                cancellationToken);

            if (exists)
                return Result<CreateAccountResponse>.Failure(AccountDomainErrors.NameAlreadyExists);

            var account = Account.Create(
                userId,
                request.CurrencyId,
                request.Name,
                request.InitialBalance,
                request.Color);

            await _accountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateAccountResponse>.Success(new CreateAccountResponse(account.Id));
        }
        catch (DomainException ex)
        {
            return Result<CreateAccountResponse>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> CreatePrimaryAsync(
        CreatePrimaryAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        try
        {
            var currency = await _currencyRepository.GetByIdAsync(request.CurrencyId, cancellationToken);
            if (currency is null)
                return Result<Guid>.Failure(AccountDomainErrors.CurrencyNotFound);

            var existsByName = await _accountRepository.ExistsByNameAsync(
                userId,
                request.Name,
                cancellationToken);

            if (existsByName)
                return Result<Guid>.Failure(AccountDomainErrors.NameAlreadyExists);

            var accounts = await _accountRepository.GetAsync(userId, cancellationToken);
            if (accounts.Any(x => x.IsPrimary))
                return Result<Guid>.Failure(AccountDomainErrors.PrimaryAccountAlreadyExists);

            var account = Account.Create(
                userId: userId,
                currencyId: request.CurrencyId,
                name: request.Name,
                initialBalance: request.InitialBalance,
                color: request.Color,
                isPrimary: true);

            await _accountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(account.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        try
        {
            var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
            if (account is null || account.UserId != userId)
                return Result<Guid>.Failure(AccountDomainErrors.AccountNotFound);

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
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        try
        {
            var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
            if (account is null || account.UserId != userId)
                return Result<Guid>.Failure(AccountDomainErrors.AccountNotFound);

            account.Archive();

            await _accountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(account.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
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
                    CurrencyId = account.CurrencyId,
                    CurrencyCode = account.Currency!.Code,
                    InitialBalance = account.InitialBalance,
                    CurrentBalance = account.InitialBalance + total,
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
            return Result<GetMonthBalanceResponse>.Failure(AccountDomainErrors.AccountNotFound);

        var monthStart = new DateTime(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var beforeTotal = await _transactionRepository.GetBalanceBeforeAsync(
            account.Id,
            monthStart,
            cancellationToken);

        var openingBalance = account.InitialBalance + beforeTotal;

        var monthChange = await _transactionRepository.GetBalanceInPeriodAsync(
            account.Id,
            monthStart,
            monthEnd,
            cancellationToken);

        var closingBalance = openingBalance + monthChange;

        return Result<GetMonthBalanceResponse>.Success(new GetMonthBalanceResponse
        {
            AccountId = account.Id,
            AccountName = account.Name,
            CurrencyCode = account.Currency!.Code,
            OpeningBalance = openingBalance,
            MonthChange = monthChange,
            ClosingBalance = closingBalance,
            Year = request.Year,
            Month = request.Month
        });
    }
}