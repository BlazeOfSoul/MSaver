using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using MSaver.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();
app.UseApiPipeline();

app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health");

app.Run();
