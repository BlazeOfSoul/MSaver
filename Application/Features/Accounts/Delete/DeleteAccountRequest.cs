namespace MSaver.Application.Features.Accounts.Delete;

public sealed record DeleteAccountRequest(Guid Id, Guid UserId);