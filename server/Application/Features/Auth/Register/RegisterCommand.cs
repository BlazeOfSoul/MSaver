namespace server.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password);