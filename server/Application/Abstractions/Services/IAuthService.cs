using server.Application.Common.Results;
using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Refresh;
using server.Application.Features.Auth.Register;

namespace server.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result<RefreshTokenResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
