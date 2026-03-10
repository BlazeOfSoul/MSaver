namespace server.Application.Features.Auth.Login;

public sealed record LoginRequest(string Email, string Password);