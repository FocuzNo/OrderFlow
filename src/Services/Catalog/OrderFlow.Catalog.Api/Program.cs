using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderFlow.Catalog.Api.ExceptionHandling;
using OrderFlow.Catalog.Api.Middleware;
using OrderFlow.Catalog.Application;
using OrderFlow.Catalog.Infrastructure;
using OrderFlow.Catalog.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog(
    (services, configuration) =>
        configuration
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "OrderFlow.Catalog")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console()
);
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder
    .Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<CatalogDbContext>("postgres", tags: ["ready"]);
builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder => resourceBuilder.AddService("OrderFlow.Catalog"))
    .WithTracing(tracingBuilder =>
        tracingBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Microsoft.EntityFrameworkCore", "Npgsql", "OrderFlow.Catalog.Kafka")
            .AddOtlpExporter()
    )
    .WithMetrics(metricsBuilder =>
        metricsBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter()
    );
var app = builder.Build();
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    return;
}
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseFastEndpoints();
app.MapOpenApi();
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions { Predicate = candidate => candidate.Tags.Contains("live") }
);
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions { Predicate = candidate => candidate.Tags.Contains("ready") }
);
app.Run();

public partial class Program;
