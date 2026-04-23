using MSaver.Api.Contracts.Transactions;

namespace MSaver.Application.Features.Transactions.Get;

public sealed class GetTransactionsRequestValidator : AbstractValidator<GetTransactionsRequest>
{
    public GetTransactionsRequestValidator()
    {
        RuleFor(x => x.Page)
            .ValidPage();

        RuleFor(x => x.Size)
            .ValidPageSize(ListQueryDefaults.MaxPageSize);

        RuleFor(x => x.SortBy)
            .ValidSortBy(TransactionSortFields.All);

        RuleFor(x => x.SortDirection)
            .ValidSortDirection();

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.AccountId)
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x.CategoryId)
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage(ValidationMessages.InvalidId);

        RuleFor(x => x)
            .ValidDateRange(x => x.FromDate, x => x.ToDate);
    }
}