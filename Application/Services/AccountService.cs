using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.CreatePrimary;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetBalance;
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
                request.Color,
                request.Icon);

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
                icon: request.Icon,
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

            var currency = await _currencyRepository.GetByIdAsync(request.CurrencyId, cancellationToken);
            if (currency is null)
                return Result<Guid>.Failure(AccountDomainErrors.CurrencyNotFound);

            var exists = await _accountRepository.ExistsByNameAsync(
                userId,
                request.Name,
                cancellationToken,
                request.Id);

            if (exists)
                return Result<Guid>.Failure(AccountDomainErrors.NameAlreadyExists);

            account.Update(request.Name, request.CurrencyId, request.Color, request.Icon);

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

    public async Task<Result<GetAccountsResponse>> GetAccountsAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var accounts = await _accountRepository.GetAsync(userId, cancellationToken);

        var items = new List<AccountItemResponse>();

        foreach (var account in accounts)
        {
            var total = await _transactionRepository.SumByAccountIdAsync(account.Id, cancellationToken);

            items.Add(new AccountItemResponse
            {
                Id = account.Id,
                Name = account.Name,
                CurrencyId = account.CurrencyId,
                CurrencyCode = account.Currency!.Code,
                InitialBalance = account.InitialBalance,
                CurrentBalance = account.InitialBalance + total,
                Color = account.Color,
                Icon = account.Icon,
                IsArchived = account.IsArchived
            });
        }

        return Result<GetAccountsResponse>.Success(new GetAccountsResponse
        {
            Items = items
        });
    }

    public async Task<Result<GetAccountBalanceResponse>> GetBalanceAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account is null || account.UserId != userId)
            return Result<GetAccountBalanceResponse>.Failure(AccountDomainErrors.AccountNotFound);

        var total = await _transactionRepository.SumByAccountIdAsync(account.Id, cancellationToken);

        return Result<GetAccountBalanceResponse>.Success(new GetAccountBalanceResponse
        {
            AccountId = account.Id,
            AccountName = account.Name,
            InitialBalance = account.InitialBalance,
            CurrentBalance = account.InitialBalance + total,
            CurrencyCode = account.Currency!.Code
        });
    }
}