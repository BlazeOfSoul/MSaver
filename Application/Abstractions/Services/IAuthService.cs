using MSaver.Application.Features.Auth.Login;
using MSaver.Application.Features.Auth.Refresh;
using MSaver.Application.Features.Auth.Register;

namespace MSaver.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<RefreshTokenResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
    Task<Result> LogoutClientAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}