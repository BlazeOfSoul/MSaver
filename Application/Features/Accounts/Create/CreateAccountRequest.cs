namespace MSaver.Application.Features.Accounts.Create;

public sealed class CreateAccountRequest
{
    public string CurrencyCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; }

    public decimal InitialBalance { get; set; }
}
