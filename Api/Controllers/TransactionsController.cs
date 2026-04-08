using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TransactionsController(
    ITransactionService transactionService,
    IValidator<CreateTransactionRequest> createValidator,
    IValidator<UpdateTransactionRequest> updateValidator) : ApiControllerBase
{
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IValidator<CreateTransactionRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateTransactionRequest> _updateValidator = updateValidator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetByUserAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync<CreateTransactionRequest, Guid>(
            request,
            _createValidator,
            ct => _transactionService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTransactionBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateTransactionRequest(
            id,
            body.AccountId,
            body.CategoryId,
            body.Amount,
            body.Date,
            body.Description,
            body.TagIds);

        return ValidateAndExecuteAsync<UpdateTransactionRequest, Guid>(
            request,
            _updateValidator,
            ct => _transactionService.UpdateAsync(request, ct),
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _transactionService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}