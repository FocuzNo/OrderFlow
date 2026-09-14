using FastEndpoints;
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

builder.Services.AddApplication().AddInfrastructure();
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseFastEndpoints();

app.Run();
