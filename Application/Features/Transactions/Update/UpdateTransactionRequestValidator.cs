using MSaver.Application.Common.Validation;

namespace MSaver.Application.Features.Transactions.Update;

public sealed class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage(ValidationMessages.MustNotBeZero);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidDate);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage(ValidationMessages.MaxLength);
    }
}