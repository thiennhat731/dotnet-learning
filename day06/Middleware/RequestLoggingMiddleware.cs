namespace day06.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"➡️ {context.Request.Method} {context.Request.Path}");
        await _next(context);
        Console.WriteLine($"⬅️ {context.Response.StatusCode}");
    }
}
