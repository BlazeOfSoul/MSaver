namespace MSaver.Api.Auth;

public sealed class AuthCookieService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public string? ReadRefreshToken(HttpRequest request)
    {
        return request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken)
            ? refreshToken
            : null;
    }

    public void AppendAuthCookies(
        HttpRequest request,
        HttpResponse response,
        string accessToken,
        string refreshToken)
    {
        response.Cookies.Append(
            AuthCookieNames.AccessToken,
            accessToken,
            CreateCookieOptions(
                request,
                TimeSpan.FromMinutes(ReadInt("JwtSettings:AccessTokenMinutes", 60))));

        response.Cookies.Append(
            AuthCookieNames.RefreshToken,
            refreshToken,
            CreateCookieOptions(
                request,
                TimeSpan.FromDays(ReadInt("JwtSettings:RefreshTokenDays", 30))));
    }

    public void ClearAuthCookies(HttpRequest request, HttpResponse response)
    {
        var options = CreateCookieOptions(request, TimeSpan.Zero);
        response.Cookies.Delete(AuthCookieNames.AccessToken, options);
        response.Cookies.Delete(AuthCookieNames.RefreshToken, options);
    }

    private CookieOptions CreateCookieOptions(HttpRequest request, TimeSpan maxAge)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = maxAge,
        };
    }

    private int ReadInt(string key, int fallback)
    {
        return int.TryParse(_configuration[key], out var value)
            ? value
            : fallback;
    }
}
