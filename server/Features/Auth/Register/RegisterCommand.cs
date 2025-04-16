using MediatR;

namespace MSaver.Api.Features.Auth.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<RegisterResponse>;
