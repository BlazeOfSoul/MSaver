using MediatR;

namespace server.Features.Auth.Login;

public record LoginQuery(string Email, string Password) : IRequest<LoginResponse>;
