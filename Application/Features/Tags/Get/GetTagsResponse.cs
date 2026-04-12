namespace MSaver.Application.Features.Tags.Get;

public sealed class GetTagsResponse
{
    public IReadOnlyCollection<TagItemResponse> Items { get; init; } = [];
}