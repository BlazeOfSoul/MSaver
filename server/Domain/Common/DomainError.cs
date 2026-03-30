namespace server.Domain.Common;

public enum DomainErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure
}

public sealed record DomainValidationItem(
    string Field,
    string Code,
    string Message);

public sealed record DomainError(
    DomainErrorType Type,
    string Code,
    string Message,
    string? Field = null,
    IReadOnlyCollection<DomainValidationItem>? Details = null)
{
    public static DomainError Validation(
        string code,
        string message,
        string? field = null,
        IReadOnlyCollection<DomainValidationItem>? details = null) =>
        new(DomainErrorType.Validation, code, message, field, details);

    public static DomainError NotFound(string code, string message, string? field = null) =>
        new(DomainErrorType.NotFound, code, message, field);

    public static DomainError Conflict(string code, string message, string? field = null) =>
        new(DomainErrorType.Conflict, code, message, field);

    public static DomainError Failure(string code, string message, string? field = null) =>
        new(DomainErrorType.Failure, code, message, field);
}