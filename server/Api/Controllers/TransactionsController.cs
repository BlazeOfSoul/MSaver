using Microsoft.AspNetCore.Mvc;
using server.Api.Extensions;
using server.Application.Services.Interfaces;
using server.Application.Features.Transactions.Create;
using server.Application.Features.Transactions.GetStatistics;

namespace server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = User.GetUserId();

        var id = await _transactionService.CreateAsync(command, cancellationToken);
        return Ok(id);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var query = new GetStatisticsQuery(userId);

        var result = await _transactionService.GetStatisticsAsync(query, cancellationToken);
        return Ok(result);
    }
}
