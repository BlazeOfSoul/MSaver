namespace MSaver.Application.Features.Auth.Login;

public sealed record LoginResponse(
    Guid Id,
    string Username,
    string Email,
    string AccessToken,
    string RefreshToken);
