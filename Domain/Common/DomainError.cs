namespace MSaver.Domain.Common;

public enum DomainErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure
}

public sealed record DomainValidationItem(
    string? Field,
    string Code,
    string Message);

public sealed record DomainError(
    DomainErrorType Type,
    string Code,
    string Message,
    IReadOnlyCollection<DomainValidationItem>? Details = null)
{
    public static DomainError Validation(
        string code,
        string message) =>
        new(DomainErrorType.Validation, code, message, null);

    public static DomainError Validation(
        string code,
        string message,
        string field) =>
        new(
            DomainErrorType.Validation,
            code,
            message,
            new[]
            {
                new DomainValidationItem(field, code, message)
            });

    public static DomainError Validation(
        string code,
        string message,
        IReadOnlyCollection<DomainValidationItem> details) =>
        new(DomainErrorType.Validation, code, message, details);

    public static DomainError NotFound(
        string code,
        string message) =>
        new(DomainErrorType.NotFound, code, message, null);

    public static DomainError Conflict(
        string code,
        string message,
        string? field = null) =>
        field is null
            ? new(DomainErrorType.Conflict, code, message, null)
            : new(
                DomainErrorType.Conflict,
                code,
                message,
                new[]
                {
                    new DomainValidationItem(field, code, message)
                });

    public static DomainError Failure(
        string code,
        string message) =>
        new(DomainErrorType.Failure, code, message, null);
}