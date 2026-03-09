using server.Application.Features.Auth.Login;
using server.Application.Features.Auth.Register;

namespace server.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginQuery query, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);
}
