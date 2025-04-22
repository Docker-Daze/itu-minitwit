using System.Diagnostics;

namespace minitwit.web;

public class LogEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogEnrichmentMiddleware> _logger;

    public LogEnrichmentMiddleware(RequestDelegate next, ILogger<LogEnrichmentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var endpoint = context.Request.Path;
        var method = context.Request.Method;
        var requestId = context.TraceIdentifier;
        var something = context.Request.Body;

        using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
        using (Serilog.Context.LogContext.PushProperty("Endpoint", endpoint))
        using (Serilog.Context.LogContext.PushProperty("Method", method))
        {
            await _next(context);
        }

        sw.Stop();
        var duration = sw.ElapsedMilliseconds;

        if (duration > 300)
        {
            _logger.LogWarning("Slow request: {Method} {Endpoint} took {Duration}ms", method, endpoint, duration);
        }

    }
}