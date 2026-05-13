namespace MSaver.Application.Features.Tags.Get;

public sealed class TagItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Color { get; init; }
}