namespace MSaver.Application.Features.Transactions.Transfer;

public sealed class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(ValidationMessages.MustBePositive);

        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .When(x => x.Rate.HasValue)
            .WithMessage(ValidationMessages.MustBePositive);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidDate);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x)
            .Must(x => x.FromAccountId != x.ToAccountId)
            .WithMessage(ValidationMessages.DifferentAccountsRequired);
    }
}
