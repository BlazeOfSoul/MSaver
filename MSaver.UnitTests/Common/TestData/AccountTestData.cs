namespace MSaver.UnitTests.Common.TestData;

public static class AccountTestData
{
    public static Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid AnotherUserId => Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static Account CreateAccount(
        Guid? userId = null,
        string currencyCode = "USD",
        string name = "Main account",
        string? color = "#111111",
        bool isPrimary = false,
        decimal initialBalance = 0m,
        DateTime? createdAt = null)
    {
        var account = Account.Create(
            userId ?? UserId,
            currencyCode,
            name,
            color,
            isPrimary,
            initialBalance);

        if (createdAt.HasValue)
            SetPrivateProperty(account, nameof(Account.CreatedAt), createdAt.Value);

        return account;
    }

    public static Dictionary<Guid, decimal> CreateTotals(params (Guid accountId, decimal balance)[] items)
    {
        return items.ToDictionary(x => x.accountId, x => x.balance);
    }

    public static PagedResult<Account> CreatePagedAccounts(
        IReadOnlyCollection<Account> items,
        int page = 1,
        int size = 20,
        int totalCount = 0)
    {
        var resolvedTotalCount = totalCount == 0 ? items.Count : totalCount;

        return new PagedResult<Account>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = resolvedTotalCount
        };
    }

    private static void SetPrivateProperty(object target, string propertyName, object value)
    {
        var type = target.GetType();
        while (type is not null)
        {
            var prop = type.GetProperty(
                propertyName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (prop is not null)
            {
                prop.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }
    }
}
