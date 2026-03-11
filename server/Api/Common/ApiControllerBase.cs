using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Results;
using server.Domain.Common;

namespace server.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return ProblemFromError(result.Error!);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return ProblemFromError(result.Error!);
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

        var payload = new
        {
            code = error.Code,
            message = error.Message,
            type = error.Type.ToString(),
            field = error.Field
        };

        return StatusCode(statusCode, payload);
    }
}
