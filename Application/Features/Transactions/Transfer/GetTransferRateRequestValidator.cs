namespace MSaver.Application.Features.Transactions.Transfer;

public sealed class GetTransferRateRequestValidator : AbstractValidator<GetTransferRateRequest>
{
    public GetTransferRateRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x)
            .Must(x => x.FromAccountId != x.ToAccountId)
            .WithMessage(ValidationMessages.DifferentAccountsRequired)
            .OverridePropertyName(nameof(GetTransferRateRequest.ToAccountId));
    }
}
