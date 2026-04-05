using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.Delete;
using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.GetBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class AccountsController(
    IAccountService accountService,
    ICurrentUserService currentUser,
    IValidator<CreateAccountRequest> createValidator,
    IValidator<UpdateAccountRequest> updateValidator) : ApiControllerBase
{
    private readonly IAccountService _accountService = accountService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IValidator<CreateAccountRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateAccountRequest> _updateValidator = updateValidator;

    [HttpGet]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var request = new GetAccountsRequest(_currentUser.UserId);
        var result = await _accountService.GetAccountsAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:guid}/balance")]
    public async Task<IActionResult> GetBalance(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new GetAccountBalanceRequest(_currentUser.UserId, id);
        var result = await _accountService.GetBalanceAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = _currentUser.UserId;

        return ValidateAndExecuteAsync<CreateAccountRequest, CreateAccountResponse>(
            request,
            _createValidator,
            ct => _accountService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        request.UserId = _currentUser.UserId;

        return ValidateAndExecuteAsync<UpdateAccountRequest, Guid>(
            request,
            _updateValidator,
            ct => _accountService.UpdateAsync(request, ct),
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new DeleteAccountRequest(id, _currentUser.UserId);
        var result = await _accountService.DeleteAsync(request, cancellationToken);
        return FromResult(result);
    }
}