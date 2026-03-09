using Microsoft.AspNetCore.Mvc;
using server.Application.Abstractions.Services;

namespace server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRatesController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRates(CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.GetExchangeRatesAsync(cancellationToken);
        return Ok(result);
    }
}
