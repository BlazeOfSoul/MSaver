using server.Api.Extensions;

namespace server.Api;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiPipeline(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices
            .GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Return on prod
        // app.UseHttpsRedirection();

        app.UseExceptionHandling();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());

        return app;
    }
}