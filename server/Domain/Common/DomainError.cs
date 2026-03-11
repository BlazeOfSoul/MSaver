namespace server.Domain.Common;

public enum DomainErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure
}

public sealed record DomainError(
    DomainErrorType Type,
    string Code,
    string Message,
    string? Field = null);
