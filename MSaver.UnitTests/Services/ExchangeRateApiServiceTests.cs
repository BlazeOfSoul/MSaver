using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MSaver.Infrastructure.Configuration;
using MSaver.Infrastructure.Services;

namespace MSaver.UnitTests.Services;

public sealed class ExchangeRateApiServiceTests
{
    [Fact]
    public async Task GetRateAsync_ShouldCacheDirectAndInverseCurrencyPairs()
    {
        var handler = new StubHttpMessageHandler(
            """
            {
                "result": "success",
                "base_code": "BYN",
                "target_code": "USD",
                "conversion_rate": 0.37
            }
            """);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new ExchangeRateApiOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.example.test/"
        });
        var sut = new ExchangeRateApiService(httpClient, options, cache);

        var first = await sut.GetRateAsync("BYN", "USD");
        var second = await sut.GetRateAsync("BYN", "USD");
        var inverse = await sut.GetRateAsync("USD", "BYN");

        first.Should().Be(0.37m);
        second.Should().Be(0.37m);
        inverse.Should().BeApproximately(2.7027027027m, 0.0000000001m);
        handler.RequestCount.Should().Be(1);
    }

    private sealed class StubHttpMessageHandler(string responseContent) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            });
        }
    }
}
