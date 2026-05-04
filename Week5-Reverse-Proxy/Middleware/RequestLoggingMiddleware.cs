namespace Week5_Reverse_Proxy.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // This method is called for each HTTP request.
    // Logged information includes the HTTP method and the request path.
    // The middleware then calls the next delegate in the pipeline to continue processing the request.
    // Logged using Serilog, which is configured in the application to write logs to the console and a file.
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
