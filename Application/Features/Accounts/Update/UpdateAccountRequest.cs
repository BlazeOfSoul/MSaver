namespace MSaver.Application.Features.Accounts.Update;

public sealed class UpdateAccountRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; }
}