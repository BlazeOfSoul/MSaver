using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Tags.Update;

public sealed class UpdateTagRequestValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Color)
            .MaximumLength(20)
            .WithMessage(ValidationMessages.MaxLength);
    }
}