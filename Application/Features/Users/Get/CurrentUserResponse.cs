namespace MSaver.Application.Features.Users.Get;

public sealed record CurrentUserResponse(
    Guid Id,
    string Username,
    string Email,
    string ApplicationCurrencyCode);
