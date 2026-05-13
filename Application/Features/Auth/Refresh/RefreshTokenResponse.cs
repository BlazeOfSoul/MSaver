namespace MSaver.Application.Features.Auth.Refresh;

public sealed record RefreshTokenResponse(
    Guid Id,
    string Username,
    string Email,
    string AccessToken,
    string RefreshToken);