namespace MSaver.Application.Features.Categories.Get;

public sealed class GetCategoriesResponse
{
    public IReadOnlyCollection<CategoryItemResponse> Items { get; init; } = [];
}