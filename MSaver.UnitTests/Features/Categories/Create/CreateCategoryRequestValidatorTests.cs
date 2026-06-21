using MSaver.Application.Features.Categories.Common;
using MSaver.Application.Features.Categories.Create;
using MSaver.Domain.Enums;

namespace MSaver.UnitTests.Features.Categories.Create;

public sealed class CreateCategoryRequestValidatorTests
{
    [Theory]
    [InlineData(CategoryType.TransferIncome)]
    [InlineData(CategoryType.TransferExpense)]
    public void Validate_ShouldReturnError_WhenTypeIsTransfer(CategoryType type)
    {
        var sut = new CreateCategoryRequestValidator();
        var request = CategoryTestData.CreateCategoryRequest(type: type);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(CreateCategoryRequest.Type) &&
            x.ErrorMessage == CategoryValidationMessages.TransferCategoryTypeIsSystemOnly);
    }
}
