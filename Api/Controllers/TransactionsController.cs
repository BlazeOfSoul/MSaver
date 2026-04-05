using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Features.Transactions.Create;
using MSaver.Application.Features.Transactions.Delete;
using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.GetStatistics;
using MSaver.Application.Features.Transactions.Update;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TransactionsController(
    ITransactionService transactionService,
    ICurrentUserService currentUser,
    IValidator<CreateTransactionRequest> createValidator,
    IValidator<UpdateTransactionRequest> updateValidator) : ApiControllerBase
{
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IValidator<CreateTransactionRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateTransactionRequest> _updateValidator = updateValidator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var request = new GetTransactionsRequest(_currentUser.UserId);

        var result = await _transactionService.GetByUserAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var request = new GetStatisticsRequest(_currentUser.UserId);

        var result = await _transactionService.GetStatisticsAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = _currentUser.UserId;

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
        request.Id = id;
        request.UserId = _currentUser.UserId;

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
        var request = new DeleteTransactionRequest(id, _currentUser.UserId);

        var result = await _transactionService.DeleteAsync(request, cancellationToken);
        return FromResult(result);
    }
}