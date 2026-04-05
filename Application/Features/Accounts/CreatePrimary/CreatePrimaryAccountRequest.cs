namespace MSaver.Application.Features.Accounts.CreatePrimary;

public sealed class CreatePrimaryAccountRequest
{
    public Guid CurrencyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal InitialBalance { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }
}