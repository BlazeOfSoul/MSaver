using FluentValidation;

using server.Application.Common.Validation;

namespace server.Application.Features.Transactions.Create;

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(ValidationMessages.MustBePositive);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage(ValidationMessages.MaxLength);
    }
}