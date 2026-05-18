namespace MSaver.Api.Common;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception occurred");
            await WriteDomainErrorAsync(context, ex.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await ApiErrorWriter.WriteAsync(
                context.Response,
                StatusCodes.Status500InternalServerError,
                ApiErrorFactory.Unexpected(),
                context.RequestAborted);
        }
    }

    private static Task WriteDomainErrorAsync(HttpContext context, DomainError error)
    {
        var statusCode = error.Type switch
        {
            DomainErrorType.Validation => StatusCodes.Status400BadRequest,
            DomainErrorType.NotFound => StatusCodes.Status404NotFound,
            DomainErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return ApiErrorWriter.WriteAsync(
            context.Response,
            statusCode,
            ApiErrorFactory.FromDomainError(error),
            context.RequestAborted);
    }
}