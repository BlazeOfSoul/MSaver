using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using server.Api.Common;
using server.Application.Abstractions.Services;

namespace server.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class ExchangeRatesController : ApiControllerBase
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
        return FromResult(result);
    }
}
