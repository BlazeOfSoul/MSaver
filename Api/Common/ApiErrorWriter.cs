using System.Text.Json;

namespace MSaver.Api.Common;

public static class ApiErrorWriter
{
    public static Task WriteAsync(
        HttpResponse response,
        int statusCode,
        ApiErrorResponse error,
        CancellationToken cancellationToken = default)
    {
        if (response.HasStarted)
            return Task.CompletedTask;

        response.Clear();
        response.StatusCode = statusCode;
        response.ContentType = "application/json; charset=utf-8";

        var json = JsonSerializer.Serialize(new
        {
            code = error.Code,
            message = error.Message,
            details = error.Details
        });

        return response.WriteAsync(json, cancellationToken);
    }
}