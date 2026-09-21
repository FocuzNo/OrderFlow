using Serilog.Context;

namespace OrderFlow.Payments.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext c)
    {
        var id =
            c.Request.Headers.TryGetValue("X-Correlation-ID", out var value)
            && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : Guid.NewGuid().ToString("N");
        c.Response.Headers["X-Correlation-ID"] = id;
        using (LogContext.PushProperty("CorrelationId", id))
            await next(c);
    }
}
