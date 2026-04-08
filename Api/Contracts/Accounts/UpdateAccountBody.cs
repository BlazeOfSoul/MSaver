namespace MSaver.Api.Contracts.Accounts;

public sealed record UpdateAccountBody(
    string Name,
    string Color);