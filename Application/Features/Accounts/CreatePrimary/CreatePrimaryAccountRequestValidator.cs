using FluentValidation;

using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Accounts.CreatePrimary;

public sealed class CreatePrimaryAccountRequestValidator : AbstractValidator<CreatePrimaryAccountRequest>
{
    #region Constructors

    public CreatePrimaryAccountRequestValidator()
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
    }

    #endregion
}