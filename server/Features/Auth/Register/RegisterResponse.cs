namespace MSaver.Api.Features.Auth.Register;

public record RegisterResponse(
    Guid Id,
    string Username,
    string Email,
    string Token
);
