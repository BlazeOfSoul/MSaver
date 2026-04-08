using FluentValidation.Results;

namespace MSaver.Api.Common;

public static class ApiErrorFactory
{
    public static ApiErrorResponse FromDomainError(DomainError error)
    {
        var details = error.Details is null
            ? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            : error.Details
                .Where(x => !string.IsNullOrWhiteSpace(x.Field))
                .GroupBy(x => x.Field!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Message).Distinct().ToArray(),
                    StringComparer.OrdinalIgnoreCase);

        return new ApiErrorResponse(
            error.Code,
            error.Message,
            details);
    }

    public static ApiErrorResponse ValidationFailed(ValidationResult validationResult)
    {
        var details = validationResult.Errors
            .Where(x => !string.IsNullOrWhiteSpace(x.PropertyName))
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return new ApiErrorResponse(
            "Validation.Failed",
            "Обнаружены ошибки валидации.",
            details);
    }

    public static ApiErrorResponse Unauthorized() =>
        new(
            "Auth.Unauthorized",
            "Пользователь не авторизован.",
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase));

    public static ApiErrorResponse Unexpected() =>
        new(
            "General.UnexpectedError",
            "Произошла непредвиденная ошибка. Попробуйте позже.",
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase));
}