using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

namespace MSaver.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult(Result result)
    {
        return result.IsSuccess ?
            NoContent() :
            ProblemFromError(result.Error!);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        return result.IsSuccess ?
            Ok(result.Value) :
            ProblemFromError(result.Error!);
    }

    protected async Task<IActionResult> ValidateAndExecuteAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var error = MapValidationErrors(validationResult);
            return ProblemFromError(error);
        }

        var result = await action(cancellationToken);
        return FromResult(result);
    }

    protected async Task<IActionResult> ValidateAndExecuteAsync<TRequest, TResponse>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<CancellationToken, Task<Result<TResponse>>> action,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var error = MapValidationErrors(validationResult);
            return ProblemFromError(error);
        }

        var result = await action(cancellationToken);
        return FromResult(result);
    }

    private static DomainError MapValidationErrors(ValidationResult validationResult)
    {
        var details = validationResult.Errors
            .Select(e => new DomainValidationItem(
                Field: e.PropertyName,
                Code: e.ErrorCode,
                Message: e.ErrorMessage))
            .ToArray();

        return DomainError.Validation(
            code: "Validation.Failed",
            message: "Обнаружены ошибки валидации.",
            details: details);
    }

    private IActionResult ProblemFromError(DomainError error)
    {
        var statusCode = error.Type switch
        {
            DomainErrorType.Validation => StatusCodes.Status400BadRequest,
            DomainErrorType.NotFound => StatusCodes.Status404NotFound,
            DomainErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var details = error.Details is null
            ? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            : error.Details
                .Where(x => !string.IsNullOrWhiteSpace(x.Field))
                .GroupBy(x => x.Field!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Message).Distinct().ToArray(),
                    StringComparer.OrdinalIgnoreCase);

        var payload = new
        {
            code = error.Code,
            message = error.Message,
            details
        };

        return StatusCode(statusCode, payload);
    }
}