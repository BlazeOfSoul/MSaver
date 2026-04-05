using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
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

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetStatisticsAsync(cancellationToken);
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
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTransactionRequest
        {
            Id = id,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description,
            TagIds = request.TagIds
        };

        return ValidateAndExecuteAsync<UpdateTransactionRequest, Guid>(
            command,
            _updateValidator,
            ct => _transactionService.UpdateAsync(command, ct),
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