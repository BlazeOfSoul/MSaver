namespace MSaver.Application.Features.Auth.Refresh;

public sealed record RefreshTokenResponse(
    Guid Id,
    string Name,
    string Email,
    string ClientId,
    string AccessToken,
    string RefreshToken);