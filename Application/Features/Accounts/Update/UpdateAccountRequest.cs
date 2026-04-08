namespace MSaver.Application.Features.Accounts.Update;

public sealed record UpdateAccountRequest(
    Guid Id,
    string Name,
    string? Color);