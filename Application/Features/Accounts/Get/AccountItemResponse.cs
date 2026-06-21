namespace MSaver.Application.Features.Accounts.Get;

public sealed class AccountItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string CurrencyCode { get; init; } = string.Empty;

    public decimal InitialBalance { get; init; }

    public decimal CurrentBalance { get; init; }

    public string? Color { get; init; }

    public bool IsArchived { get; init; }

    public bool IsPrimary { get; init; }
}
