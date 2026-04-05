using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Accounts.Create;

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.CurrencyId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationMessages.MustBeZeroOrPositive);

        RuleFor(x => x.Color)
            .MaximumLength(20)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.Icon)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.MaxLength);
    }
}