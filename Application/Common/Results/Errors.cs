namespace MSaver.Application.Common.Results;

public static class Errors
{
    public static Error Validation(string code, string message) =>
        new(ErrorType.Validation, code, message);

    public static Error NotFound(string code, string message) =>
        new(ErrorType.NotFound, code, message);

    public static Error Conflict(string code, string message) =>
        new(ErrorType.Conflict, code, message);

    public static Error Failure(string code, string message) =>
        new(ErrorType.Failure, code, message);
}
