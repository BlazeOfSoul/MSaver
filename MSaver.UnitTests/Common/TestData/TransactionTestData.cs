using System.Reflection;

using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Transfer;
using MSaver.Application.Features.Transactions.Update;
using MSaver.Domain.Enums;

namespace MSaver.UnitTests.Common.TestData;

public static class TransactionTestData
{
    public static Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid AnotherUserId => Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static User CreateUser(Guid? id = null)
    {
        var user = User.Create("Test User", "test@example.com", "hashed-password");
        SetId(user, id ?? UserId);
        return user;
    }

    public static Account CreateAccount(
        Guid userId,
        string currencyCode = "USD",
        string name = "Main",
        string? color = "#FF0000",
        bool isPrimary = false,
        bool isArchived = false,
        Guid? id = null)
    {
        var account = Account.Create(userId, currencyCode, name, color, isPrimary);
        SetId(account, id ?? Guid.NewGuid());

        if (isArchived)
            account.Archive();

        return account;
    }

    public static Category CreateCategory(
        Guid userId,
        string name = "Food",
        CategoryType type = CategoryType.Debit,
        string color = "#FF0000",
        bool isDeleted = false,
        Guid? id = null)
    {
        var category = Category.Create(userId, name, type, color);
        SetId(category, id ?? Guid.NewGuid());

        if (isDeleted && !category.IsSystem)
            category.SoftDelete();

        return category;
    }

    public static Transaction CreateTransaction(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        decimal amount = -10m,
        DateTime? date = null,
        string? description = "Coffee",
        Guid? id = null,
        Account? account = null,
        Category? category = null,
        Guid? transferId = null)
    {
        var transaction = Transaction.Create(
            userId,
            accountId,
            categoryId,
            amount,
            date ?? new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            description,
            transferId);

        SetId(transaction, id ?? Guid.NewGuid());

        if (account is not null)
            SetPrivateProperty(transaction, nameof(Transaction.Account), account);

        if (category is not null)
            SetPrivateProperty(transaction, nameof(Transaction.Category), category);

        return transaction;
    }

    public static CreateTransactionRequest CreateTransactionRequest(
        Guid? accountId = null,
        Guid? categoryId = null,
        decimal amount = -10m,
        DateTime? date = null,
        string description = "Coffee")
    {
        return new CreateTransactionRequest
        {
            AccountId = accountId ?? Guid.NewGuid(),
            CategoryId = categoryId ?? Guid.NewGuid(),
            Amount = amount,
            Date = date ?? new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Description = description
        };
    }

    public static UpdateTransactionRequest CreateUpdateTransactionRequest(
        Guid? id = null,
        Guid? categoryId = null,
        decimal amount = -20m,
        DateTime? date = null,
        string? description = "Updated")
    {
        return new UpdateTransactionRequest(
            id ?? Guid.NewGuid(),
            categoryId ?? Guid.NewGuid(),
            amount,
            date ?? new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc),
            description);
    }

    public static CreateTransferRequest CreateTransferRequest(
        Guid? fromAccountId = null,
        Guid? toAccountId = null,
        decimal amount = 100m,
        DateTime? date = null,
        decimal? rate = null,
        string? description = "Transfer")
    {
        return new CreateTransferRequest(
            fromAccountId ?? Guid.NewGuid(),
            toAccountId ?? Guid.NewGuid(),
            amount,
            date ?? new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc),
            rate,
            description);
    }

    public static GetTransactionsRequest CreateGetTransactionsRequest(
        Guid? accountId = null,
        Guid? categoryId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = ListQueryDefaults.DefaultPage,
        int size = ListQueryDefaults.DefaultPageSize)
    {
        return new GetTransactionsRequest
        {
            AccountId = accountId,
            CategoryId = categoryId,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Page = page,
            Size = size
        };
    }

    public static PagedResult<Transaction> CreatePagedTransactions(
        IReadOnlyCollection<Transaction> items,
        int page = ListQueryDefaults.DefaultPage,
        int size = ListQueryDefaults.DefaultPageSize,
        int? totalCount = null)
    {
        return new PagedResult<Transaction>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = totalCount ?? items.Count
        };
    }

    private static void SetId(object entity, Guid id)
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var prop = type.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop is not null)
            {
                prop.SetValue(entity, id);
                return;
            }
            type = type.BaseType;
        }
    }

    private static void SetPrivateProperty(object target, string propertyName, object value)
    {
        var type = target.GetType();
        while (type is not null)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop is not null)
            {
                prop.SetValue(target, value);
                return;
            }
            type = type.BaseType;
        }
    }
}
