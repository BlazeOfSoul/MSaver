using MediatR;
using Microsoft.AspNetCore.Mvc;

using server.Extensions;
using server.Features.Transactions.Create;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionCommand command)
    {
        command.UserId = User.GetUserId();
        var id = await _mediator.Send(command);
        return Ok(id);
    }
}
