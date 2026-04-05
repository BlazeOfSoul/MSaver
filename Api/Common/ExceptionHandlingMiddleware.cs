using System.Text.Json;

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
            await WriteUnexpectedErrorAsync(context);
        }
    }

    private static async Task WriteDomainErrorAsync(HttpContext context, DomainError error)
    {
        if (context.Response.HasStarted)
            return;

        var statusCode = error.Type switch
        {
            DomainErrorType.Validation => StatusCodes.Status400BadRequest,
            DomainErrorType.NotFound => StatusCodes.Status404NotFound,
            DomainErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var details = error.Details is null
            ? [with(StringComparer.OrdinalIgnoreCase)]
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

        var json = JsonSerializer.Serialize(payload);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(json);
    }

    private static async Task WriteUnexpectedErrorAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            return;

        var payload = new
        {
            code = "General.UnexpectedError",
            message = "Произошла непредвиденная ошибка. Попробуйте позже.",
            details = new Dictionary<string, string[]>()
        };

        var json = JsonSerializer.Serialize(payload);

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(json);
    }
}