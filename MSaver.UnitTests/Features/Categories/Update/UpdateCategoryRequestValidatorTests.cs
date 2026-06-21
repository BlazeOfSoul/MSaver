using MSaver.Application.Features.Categories.Common;
using MSaver.Application.Features.Categories.Update;
using MSaver.Domain.Enums;

namespace MSaver.UnitTests.Features.Categories.Update;

public sealed class UpdateCategoryRequestValidatorTests
{
    [Theory]
    [InlineData(CategoryType.TransferIncome)]
    [InlineData(CategoryType.TransferExpense)]
    public void Validate_ShouldReturnError_WhenTypeIsTransfer(CategoryType type)
    {
        var sut = new UpdateCategoryRequestValidator();
        var request = CategoryTestData.CreateUpdateCategoryRequest(type: type);

        var result = sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(UpdateCategoryRequest.Type) &&
            x.ErrorMessage == CategoryValidationMessages.TransferCategoryTypeIsSystemOnly);
    }
}
