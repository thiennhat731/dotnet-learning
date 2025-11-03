using System.Net;
using System.Text.Json;
using CollabDoc.Application.Common;

namespace CollabDoc.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[GlobalExceptionMiddleware] {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();

            _logger.LogError(ex, "❌ Global exception caught: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var error = "Internal Server Error";
        var message = exception.Message;

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                error = "Bad Request";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                error = "Not Found";
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                error = "Unauthorized";
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.Conflict;
                error = "Conflict";
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                error = "Validation Failed";
                message = string.Join("; ", validationEx.Errors);
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var apiResponse = new ApiResponse<string>(
            (int)statusCode,
            message,
            null,
            error
        );

        var json = JsonSerializer.Serialize(apiResponse,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
