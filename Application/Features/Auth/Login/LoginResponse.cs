namespace MSaver.Application.Features.Auth.Login;

public sealed record LoginResponse(
    Guid Id,
    string Name,
    string Email,
    string ClientId,
    string AccessToken,
    string RefreshToken);
