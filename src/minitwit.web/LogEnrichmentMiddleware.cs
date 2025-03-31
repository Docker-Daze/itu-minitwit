namespace minitwit.web;

public class LogEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public LogEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path;
        var method = context.Request.Method;
        var requestId = context.TraceIdentifier;

        using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
        using (Serilog.Context.LogContext.PushProperty("Endpoint", endpoint))
        using (Serilog.Context.LogContext.PushProperty("Method", method))
        {
            await _next(context);
        }
    }
}
