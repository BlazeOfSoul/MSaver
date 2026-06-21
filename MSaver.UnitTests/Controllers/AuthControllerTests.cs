using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using MSaver.Api.Auth;
using MSaver.Api.Controllers;
using MSaver.Application.Abstractions.Services;
using MSaver.Application.Common.Results;

namespace MSaver.UnitTests.Controllers;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IValidator<RegisterRequest>> _registerValidator = new();
    private readonly Mock<IValidator<LoginRequest>> _loginValidator = new();
    private readonly Mock<IValidator<RefreshTokenRequest>> _refreshValidator = new();

    [Fact]
    public async Task Login_ShouldSetHttpOnlyAuthCookiesAndReturnSessionWithoutTokens()
    {
        var request = AuthTestData.CreateLoginRequest();
        var response = new LoginResponse(
            Guid.NewGuid(),
            "Alex",
            "alex@example.com",
            "client-id",
            "access-token",
            "refresh-token");
        var sut = CreateSut();

        _loginValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _authService
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResponse>.Success(response));

        var result = await sut.Login(request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new
        {
            response.Id,
            response.Name,
            response.Email,
            response.ClientId,
        });
        ok.Value!.GetType().GetProperty("AccessToken").Should().BeNull();
        ok.Value.GetType().GetProperty("RefreshToken").Should().BeNull();

        var setCookieHeaders = ReadSetCookieHeaders(sut);
        setCookieHeaders.Should().Contain(cookie =>
            cookie.StartsWith($"{AuthCookieNames.AccessToken}=access-token;") &&
            cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        setCookieHeaders.Should().Contain(cookie =>
            cookie.StartsWith($"{AuthCookieNames.RefreshToken}=refresh-token;") &&
            cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Refresh_ShouldReadRefreshTokenFromCookieAndRotateAuthCookies()
    {
        var response = new RefreshTokenResponse(
            Guid.NewGuid(),
            "Alex",
            "alex@example.com",
            "client-id",
            "next-access-token",
            "next-refresh-token");
        var sut = CreateSut($"{AuthCookieNames.RefreshToken}=current-refresh-token");

        _refreshValidator
            .Setup(x => x.ValidateAsync(
                It.Is<RefreshTokenRequest>(request => request.RefreshToken == "current-refresh-token"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _authService
            .Setup(x => x.RefreshAsync(
                It.Is<RefreshTokenRequest>(request => request.RefreshToken == "current-refresh-token"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshTokenResponse>.Success(response));

        var result = await sut.Refresh(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new
        {
            response.Id,
            response.Name,
            response.Email,
            response.ClientId,
        });
        ok.Value!.GetType().GetProperty("AccessToken").Should().BeNull();
        ok.Value.GetType().GetProperty("RefreshToken").Should().BeNull();

        var setCookieHeaders = ReadSetCookieHeaders(sut);
        setCookieHeaders.Should().Contain(cookie =>
            cookie.StartsWith($"{AuthCookieNames.AccessToken}=next-access-token;"));
        setCookieHeaders.Should().Contain(cookie =>
            cookie.StartsWith($"{AuthCookieNames.RefreshToken}=next-refresh-token;"));
    }

    [Fact]
    public async Task Logout_ShouldUseCurrentClientIdAndClearAuthCookies()
    {
        var userId = Guid.NewGuid();
        var sut = CreateSut(
            $"{AuthCookieNames.AccessToken}=access-token; {AuthCookieNames.RefreshToken}=refresh-token");

        _currentUserService.SetupGet(x => x.UserId).Returns(userId);
        _currentUserService.SetupGet(x => x.ClientId).Returns("client-id");
        _authService
            .Setup(x => x.LogoutClientAsync(userId, "client-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await sut.Logout(CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _authService.Verify(
            x => x.LogoutClientAsync(userId, "client-id", It.IsAny<CancellationToken>()),
            Times.Once);

        var setCookieHeaders = ReadSetCookieHeaders(sut);
        setCookieHeaders.Should().Contain(cookie =>
            cookie.StartsWith($"{AuthCookieNames.AccessToken}=;") &&
            cookie.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase));
        setCookieHeaders.Should().Contain(cookie =>
            cookie.StartsWith($"{AuthCookieNames.RefreshToken}=;") &&
            cookie.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase));
    }

    private AuthController CreateSut(string? cookieHeader = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:AccessTokenMinutes"] = "60",
                ["JwtSettings:RefreshTokenDays"] = "30",
            })
            .Build();

        var controller = new AuthController(
            _authService.Object,
            _currentUserService.Object,
            _registerValidator.Object,
            _loginValidator.Object,
            _refreshValidator.Object,
            new AuthCookieService(configuration));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";

        if (cookieHeader is not null)
        {
            httpContext.Request.Headers.Cookie = cookieHeader;
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };

        return controller;
    }

    private static IReadOnlyCollection<string> ReadSetCookieHeaders(AuthController controller)
    {
        return controller.Response.Headers.SetCookie.Select(x => x ?? string.Empty).ToArray();
    }
}
