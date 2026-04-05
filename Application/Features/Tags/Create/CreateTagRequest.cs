namespace MSaver.Application.Features.Tags.Create;

public sealed class CreateTagRequest
{
    public Guid UserId
    {
        get; set;
    }

    public string Name
    {
        get; set;
    } = string.Empty;

    public string? Color
    {
        get; set;
    }
}