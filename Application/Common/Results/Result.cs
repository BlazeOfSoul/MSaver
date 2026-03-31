using MSaver.Domain.Common;

namespace MSaver.Application.Common.Results;

public class Result
{
    protected Result(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess
    {
        get;
    }
    public bool IsFailure => !IsSuccess;
    public DomainError? Error
    {
        get;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(DomainError error) => new(false, error);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, DomainError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value
    {
        get;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static new Result<T> Failure(DomainError error) => new(false, default, error);
}
