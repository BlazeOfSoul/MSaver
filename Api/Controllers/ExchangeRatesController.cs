using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;
using MSaver.Application.Abstractions.Services;

namespace MSaver.Api.Controllers;

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
