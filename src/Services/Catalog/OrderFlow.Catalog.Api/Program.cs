using FastEndpoints;
using OrderFlow.Catalog.Api;
using OrderFlow.Catalog.Application;
using OrderFlow.Catalog.Infrastructure;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddApplication().AddInfrastructure(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseFastEndpoints();

app.Run();
