using MediatR;
using Microsoft.AspNetCore.Mvc;
using server.Features.ExchangeRates;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangeRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRates()
    {
        var result = await _mediator.Send(new GetExchangeRatesQuery());
        return Ok(result);
    }
}