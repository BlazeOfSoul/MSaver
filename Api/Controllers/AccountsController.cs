using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Api.Contracts.Accounts;
using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.GetMonthBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class AccountsController(
    IAccountService accountService,
    IValidator<CreateAccountRequest> createValidator,
    IValidator<UpdateAccountRequest> updateValidator,
    IValidator<GetMonthBalanceRequest> getMonthBalanceValidator) : ApiControllerBase
{
    private readonly IAccountService _accountService = accountService;
    private readonly IValidator<CreateAccountRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateAccountRequest> _updateValidator = updateValidator;
    private readonly IValidator<GetMonthBalanceRequest> _getMonthBalanceValidator = getMonthBalanceValidator;

    [HttpGet]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var result = await _accountService.GetAccountsAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{accountId:guid}/month-balance")]
    public Task<IActionResult> GetMonthBalance(
        Guid accountId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var request = new GetMonthBalanceRequest(
            accountId,
            year,
            month);

        return ValidateAndExecuteAsync(
            request,
            _getMonthBalanceValidator,
            ct => _accountService.GetMonthBalanceAsync(request, ct),
            cancellationToken);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _createValidator,
            ct => _accountService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccountBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateAccountRequest(
            id,
            body.Name,
            body.Color);

        return ValidateAndExecuteAsync(
            request,
            _updateValidator,
            ct => _accountService.UpdateAsync(request, ct),
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}