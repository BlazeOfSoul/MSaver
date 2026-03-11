namespace server.Application.Common.Results;

public sealed record Error(
    ErrorType Type,
    string Code,
    string Message);