using MSaver.Api.Contracts.Categories;
using MSaver.Application.Features.Categories.Common;
using MSaver.Application.Features.Categories.Get;

namespace MSaver.UnitTests.Features.Categories.Get;

public sealed class GetCategoriesRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenTypeIsUndefinedNumericValue()
    {
        var sut = new GetCategoriesRequestValidator();
        var request = new GetCategoriesRequest
        {
            Type = "999"
        };

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(GetCategoriesRequest.Type) &&
            x.ErrorMessage == CategoryValidationMessages.InvalidCategoryType);
    }
}
