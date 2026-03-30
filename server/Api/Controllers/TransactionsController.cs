using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using server.Api.Common;
using server.Application.Abstractions.Auth;
using server.Application.Abstractions.Services;
using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;

namespace server.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TransactionsController : ApiControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateTransactionRequest> _createValidator;

    public TransactionsController(
        ITransactionService transactionService,
        ICurrentUserService currentUser,
        IValidator<CreateTransactionRequest> createValidator)
    {
        _transactionService = transactionService;
        _currentUser = currentUser;
        _createValidator = createValidator;
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

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var request = new GetStatisticsRequest(userId);

        var result = await _transactionService.GetStatisticsAsync(request, cancellationToken);
        return FromResult(result);
    }
}