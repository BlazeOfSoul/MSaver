namespace MSaver.Application.Features.Auth.Login;

public sealed record LoginResponse(
    Guid Id,
    string AccessToken,
    string RefreshToken);