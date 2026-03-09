namespace server.Application.Features.Auth.Register;

public sealed record RegisterResponse(
    Guid Id,
    string Username,
    string Email,
    string Token);