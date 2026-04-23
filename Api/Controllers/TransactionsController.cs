using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Contracts.Transactions;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Transfer;
using MSaver.Application.Features.Transactions.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TransactionsController(
    ITransactionService transactionService,
    IValidator<GetTransactionsRequest> getValidator,
    IValidator<CreateTransactionRequest> createValidator,
    IValidator<CreateTransferRequest> transferValidator,
    IValidator<UpdateTransactionRequest> updateValidator) : ApiControllerBase
{
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IValidator<GetTransactionsRequest> _getValidator = getValidator;
    private readonly IValidator<CreateTransactionRequest> _createValidator = createValidator;
    private readonly IValidator<CreateTransferRequest> _transferValidator = transferValidator;
    private readonly IValidator<UpdateTransactionRequest> _updateValidator = updateValidator;

    [HttpGet]
    public Task<IActionResult> Get(
        [FromQuery] GetTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _getValidator,
            ct => _transactionService.GetAsync(request, ct),
            cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        return ValidateAndExecuteAsync(
            request,
            _createValidator,
            ct => _transactionService.CreateAsync(request, ct),
            cancellationToken);
    }

    [HttpPost("transfer")]
    public Task<IActionResult> Transfer(
        [FromBody] CreateTransferBody body,
        CancellationToken cancellationToken)
    {
        var request = new CreateTransferRequest(
            body.FromAccountId,
            body.ToAccountId,
            body.Amount,
            body.Date,
            body.Rate,
            body.Description);

        return ValidateAndExecuteAsync(
            request,
            _transferValidator,
            ct => _transactionService.TransferAsync(request, ct),
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
            body.CategoryId,
            body.Amount,
            body.Date,
            body.Description);

        return ValidateAndExecuteAsync(
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