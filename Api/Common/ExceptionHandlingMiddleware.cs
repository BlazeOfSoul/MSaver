using System.Text.Json;

using MSaver.Domain.Common;

using Microsoft.AspNetCore.Http;

namespace MSaver.Api.Common;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

        var payload = new
        {
            code = error.Code,
            message = error.Message,
            type = error.Type.ToString(),
            field = error.Field
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
            type = DomainErrorType.Failure.ToString(),
            field = (string?)null
        };

        var json = JsonSerializer.Serialize(payload);

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(json);
    }
}
