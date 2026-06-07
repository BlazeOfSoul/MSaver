using MSaver.Application.Features.Categories.Common;
using MSaver.Domain.Enums;

namespace MSaver.UnitTests.Validation;

public sealed class CategoryTypeHelperTests
{
    [Fact]
    public void IsValid_ShouldReturnFalse_WhenValueIsUndefinedEnumNumber()
    {
        CategoryTypeHelper.IsValid("999").Should().BeFalse();
    }

    [Fact]
    public void TryParse_ShouldReturnFalse_WhenValueIsUndefinedEnumNumber()
    {
        var result = CategoryTypeHelper.TryParse("999", out var type);

        result.Should().BeFalse();
        type.Should().Be(default(CategoryType));
    }
}
