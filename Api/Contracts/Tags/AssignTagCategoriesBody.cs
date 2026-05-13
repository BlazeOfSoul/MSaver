namespace MSaver.Api.Contracts.Tags;

public sealed record AssignTagCategoriesBody(
    IReadOnlyCollection<Guid> CategoryIds);