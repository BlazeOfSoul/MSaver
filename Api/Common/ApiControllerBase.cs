using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult(Result result)
    {
        return result.IsSuccess
            ? NoContent()
            : ProblemFromError(result.Error!);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        return result.IsSuccess
            ? Ok(result.Value)
            : ProblemFromError(result.Error!);
    }

    protected async Task<IActionResult> ValidateAndExecuteAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(ApiErrorFactory.ValidationFailed(validationResult));

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
            return BadRequest(ApiErrorFactory.ValidationFailed(validationResult));

        var result = await action(cancellationToken);
        return FromResult(result);
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

        return StatusCode(statusCode, ApiErrorFactory.FromDomainError(error));
    }
}