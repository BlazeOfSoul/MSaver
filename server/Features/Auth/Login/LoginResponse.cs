namespace server.Features.Auth.Login;

public record LoginResponse(Guid Id, string Username, string Email, string Token);
