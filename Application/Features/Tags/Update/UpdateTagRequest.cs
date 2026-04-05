namespace MSaver.Application.Features.Tags.Update;

public sealed class UpdateTagRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; }
}