using Microsoft.AspNetCore.Mvc;
using server.Api.Common;
using server.Api.Extensions;
using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;
using server.Application.Services.Interfaces;

namespace server.Api.Controllers;

[Route("api/[controller]")]
public sealed class TransactionsController : ApiControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        request.UserId = User.GetUserId();

        var result = await _transactionService.CreateAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var request = new GetStatisticsRequest(userId);

        var result = await _transactionService.GetStatisticsAsync(request, cancellationToken);
        return FromResult(result);
    }
}
