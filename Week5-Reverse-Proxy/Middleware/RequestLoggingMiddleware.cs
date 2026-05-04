using Serilog;
using System.Diagnostics;
using ILogger = Serilog.ILogger;

namespace Week5_Reverse_Proxy.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // This method is called for each HTTP request.
    // Logged information includes the HTTP method and the request path.
    // The middleware then calls the next delegate in the pipeline to continue processing the request.
    // Logged using Serilog, which is configured in the application to write logs to the console and a file.
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation("{Method} {Path}{QueryString} -> {StatusCode} in {Elapsed}ms",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
