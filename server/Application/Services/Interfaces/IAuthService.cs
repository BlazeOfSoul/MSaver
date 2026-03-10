using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Register;

namespace server.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest query, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest command, CancellationToken cancellationToken = default);
}
