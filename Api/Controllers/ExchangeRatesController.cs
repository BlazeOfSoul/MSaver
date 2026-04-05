using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MSaver.Api.Common;

namespace MSaver.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class ExchangeRatesController(IExchangeRateService exchangeRateService) : ApiControllerBase
{
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;

    [HttpGet]
    public async Task<IActionResult> GetRates(CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.GetExchangeRatesAsync(cancellationToken);
        return FromResult(result);
    }
}