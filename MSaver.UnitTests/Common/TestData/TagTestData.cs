using System.Reflection;

using MSaver.Api.Contracts.Tags;
using MSaver.Application.Features.Tags.AssignCategories;
using MSaver.Application.Features.Tags.Create;
using MSaver.Application.Features.Tags.Update;

namespace MSaver.UnitTests.Common.TestData;

public static class TagTestData
{
    public static Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid AnotherUserId => Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static GetTagsRequest CreateGetTagsRequest(
        string? search = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = ListQueryDefaults.DefaultPage,
        int size = ListQueryDefaults.DefaultPageSize)
    {
        return new GetTagsRequest
        {
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Page = page,
            Size = size
        };
    }

    public static CreateTagRequest CreateTagRequest(
        string name = "Important",
        string? color = "#FF0000")
    {
        return new CreateTagRequest
        {
            Name = name,
            Color = color
        };
    }

    public static UpdateTagRequest CreateUpdateTagRequest(
        Guid? id = null,
        string name = "Updated tag",
        string? color = "#00FF00")
    {
        return new UpdateTagRequest(
            id ?? Guid.NewGuid(),
            name,
            color);
    }

    public static AssignTagCategoriesRequest CreateAssignCategoriesRequest(
        Guid? tagId = null,
        IReadOnlyCollection<Guid>? categoryIds = null)
    {
        return new AssignTagCategoriesRequest
        {
            TagId = tagId ?? Guid.NewGuid(),
            CategoryIds = categoryIds ?? Array.Empty<Guid>()
        };
    }

    public static Tag CreateTag(
        Guid userId,
        string name = "Important",
        string? color = "#FF0000")
    {
        return Tag.Create(userId, name, color);
    }

    public static User CreateUser(Guid id)
    {
        var user = User.Create(
            name: "Test User",
            email: "[email protected]",
            passwordHash: "hashed-password");

        SetPrivateProperty(user, "Id", id);
        return user;
    }

    public static PagedResult<Tag> CreatePagedTags(
        IReadOnlyCollection<Tag> items,
        int page = ListQueryDefaults.DefaultPage,
        int size = ListQueryDefaults.DefaultPageSize,
        int totalCount = 0)
    {
        return new PagedResult<Tag>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = totalCount == 0 ? items.Count : totalCount
        };
    }

    public static Tag CreateTagWithCategories(
        Guid userId,
        string name,
        string? color,
        params Category[] categories)
    {
        var tag = Tag.Create(userId, name, color);

        var tagId = GetEntityId(tag);
        if (tagId == Guid.Empty)
        {
            tagId = Guid.NewGuid();
            SetPrivateProperty(tag, "Id", tagId);
        }

        var tagCategories = new List<TagCategory>();

        foreach (var category in categories)
        {
            var tagCategory = TagCategory.Create(tagId, category.Id);
            SetPrivateProperty(tagCategory, "Category", category);
            SetPrivateProperty(tagCategory, "Tag", tag);
            tagCategories.Add(tagCategory);
        }

        SetBackingField(tag, "_tagCategories", tagCategories);

        return tag;
    }

    public static Tag CreateTagWithCategories(
        Guid userId,
        string name,
        string? color,
        Category first,
        Category second,
        Category third,
        bool includeNullCategoryLink)
    {
        var tag = CreateTagWithCategories(userId, name, color, first, second, third);

        if (includeNullCategoryLink)
        {
            var tagId = GetEntityId(tag);
            var nullLink = TagCategory.Create(tagId, Guid.NewGuid());
            SetBackingFieldAppend(tag, "_tagCategories", nullLink);
        }

        return tag;
    }

    private static Guid GetEntityId(object entity)
    {
        var prop = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop is null)
            return Guid.Empty;

        var value = prop.GetValue(entity);
        return value is Guid guid ? guid : Guid.Empty;
    }

    private static void SetPrivateProperty(object target, string propertyName, object? value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop?.SetValue(target, value);
    }

    private static void SetBackingField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }

    private static void SetBackingFieldAppend(object target, string fieldName, TagCategory value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(target) is IList<TagCategory> list)
        {
            list.Add(value);
        }
    }
}