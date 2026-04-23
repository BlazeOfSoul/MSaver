namespace MSaver.Application.Features.Transactions.Create;

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId)
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