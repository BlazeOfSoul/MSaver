namespace MSaver.Application.Features.Accounts.Create;

public sealed class CreateAccountRequest
{
    public Guid CurrencyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal InitialBalance { get; set; }

    public string? Color { get; set; }
}