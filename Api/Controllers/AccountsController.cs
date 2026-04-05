using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Accounts.Create;
using MSaver.Application.Features.Accounts.CreatePrimary;
using MSaver.Application.Features.Accounts.GetMonthBalance;
using MSaver.Application.Features.Accounts.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class AccountsController(
    IAccountService accountService,
    IValidator<CreateAccountRequest> createValidator,
    IValidator<CreatePrimaryAccountRequest> createPrimaryValidator,
    IValidator<UpdateAccountRequest> updateValidator,
    IValidator<GetMonthBalanceRequest> getMonthBalanceValidator) : ApiControllerBase
{
    private readonly IAccountService _accountService = accountService;
    private readonly IValidator<CreateAccountRequest> _createValidator = createValidator;
    private readonly IValidator<CreatePrimaryAccountRequest> _createPrimaryValidator = createPrimaryValidator;
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
        var request = new GetMonthBalanceRequest
        {
            AccountId = accountId,
            Year = year,
            Month = month
        };

        return ValidateAndExecuteAsync<GetMonthBalanceRequest, GetMonthBalanceResponse>(
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
        return ValidateAndExecuteAsync<CreateAccountRequest, CreateAccountResponse>(
            request,
            _createValidator,
            ct => _accountService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPost("primary")]
    public Task<IActionResult> CreatePrimary(
        [FromBody] CreatePrimaryAccountRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync<CreatePrimaryAccountRequest, Guid>(
            request,
            _createPrimaryValidator,
            ct => _accountService.CreatePrimaryAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = id;

        return ValidateAndExecuteAsync<UpdateAccountRequest, Guid>(
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