using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.Delete;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Application.Services;

public sealed class AccountService(
    IAccountRepository accountRepository,
    ICurrencyRepository currencyRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICurrencyRepository _currencyRepository = currencyRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CreateAccountResponse>> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currency = await _currencyRepository.GetByIdAsync(request.CurrencyId, cancellationToken);
            if (currency is null)
                return Result<CreateAccountResponse>.Failure(AccountDomainErrors.CurrencyNotFound);

            var exists = await _accountRepository.ExistsByNameAsync(
                request.UserId,
                request.Name,
                cancellationToken);

            if (exists)
                return Result<CreateAccountResponse>.Failure(AccountDomainErrors.NameAlreadyExists);

            var account = Account.Create(
                request.UserId,
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

    public async Task<Result<Guid>> UpdateAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
            if (account is null || account.UserId != request.UserId)
                return Result<Guid>.Failure(AccountDomainErrors.AccountNotFound);

            var currency = await _currencyRepository.GetByIdAsync(request.CurrencyId, cancellationToken);
            if (currency is null)
                return Result<Guid>.Failure(AccountDomainErrors.CurrencyNotFound);

            var exists = await _accountRepository.ExistsByNameAsync(
                request.UserId,
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
        DeleteAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null || account.UserId != request.UserId)
            return Result<Guid>.Failure(AccountDomainErrors.AccountNotFound);

        account.Archive();

        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(account.Id);
    }

    public async Task<Result<GetAccountsResponse>> GetAccountsAsync(
        GetAccountsRequest request,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetAsync(request.UserId, cancellationToken);

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
        GetAccountBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null || account.UserId != request.UserId)
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