using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Accounts.Update;

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.CurrencyId)
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

        RuleFor(x => x.Icon)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.MaxLength);
    }
}