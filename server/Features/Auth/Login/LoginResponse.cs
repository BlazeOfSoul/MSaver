namespace MSaver.Api.Features.Auth.Login;

public record LoginResponse(Guid Id, string Username, string Email, string Token);
