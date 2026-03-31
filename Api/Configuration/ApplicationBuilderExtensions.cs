using MSaver.Api.Extensions;

namespace MSaver.Api.Configuration;

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

        // app.UseHttpsRedirection();

        app.UseExceptionHandling();

        app.UseRouting();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());

        return app;
    }
}