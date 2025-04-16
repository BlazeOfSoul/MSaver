using MediatR;

namespace MSaver.Api.Features.Auth.Login;

public record LoginQuery(string Email, string Password) : IRequest<LoginResponse>;
