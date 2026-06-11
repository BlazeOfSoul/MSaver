using System.Net;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
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
            HttpStatusCode.OK,
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
        var sut = new ExchangeRateApiService(
            httpClient,
            options,
            cache,
            NullLogger<ExchangeRateApiService>.Instance);

        var first = await sut.GetRateAsync("BYN", "USD");
        var second = await sut.GetRateAsync("BYN", "USD");
        var inverse = await sut.GetRateAsync("USD", "BYN");

        first.Should().Be(0.37m);
        second.Should().Be(0.37m);
        inverse.Should().BeApproximately(2.7027027027m, 0.0000000001m);
        handler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRateAsync_ShouldNotExposeRawProviderResponse_WhenHttpErrorOccurs()
    {
        const string providerResponse = "provider-secret-details";
        var handler = new StubHttpMessageHandler(HttpStatusCode.BadRequest, providerResponse);
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
        var sut = new ExchangeRateApiService(
            httpClient,
            options,
            cache,
            NullLogger<ExchangeRateApiService>.Instance);

        var action = async () => await sut.GetRateAsync("BYN", "USD");

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*400*")
            .Where(ex => !ex.Message.Contains(providerResponse, StringComparison.Ordinal));
    }

    private sealed class StubHttpMessageHandler(
        HttpStatusCode statusCode,
        string responseContent) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            });
        }
    }
}
