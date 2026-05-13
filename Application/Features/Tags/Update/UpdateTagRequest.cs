namespace MSaver.Application.Features.Tags.Update;

public sealed record UpdateTagRequest(
    Guid Id,
    string Name,
    string? Color);