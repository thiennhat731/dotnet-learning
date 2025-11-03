using System.Net;
using System.Text.Json;
using CollabDoc.Application.Common;

namespace CollabDoc.Api.Middlewares;

public class FormatResponseMiddleware
{
    private readonly RequestDelegate _next;

    public FormatResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        try
        {
            await _next(context);

            // ⚠️ Nếu status khác 200/201 → KHÔNG format → trả về như GlobalExceptionMiddleware đã xử lý
            if (context.Response.StatusCode != 200 && context.Response.StatusCode != 201)
            {
                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);
                return;
            }

            newBody.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(newBody).ReadToEndAsync();
            newBody.Seek(0, SeekOrigin.Begin);

            if (string.IsNullOrWhiteSpace(bodyText))
            {
                context.Response.Body = originalBody;
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/swagger") || path.StartsWith("/v3/api-docs"))
            {
                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);
                return;
            }
            if (context.Request.Path.StartsWithSegments("/hubs"))
            {
                await _next(context);
                return;
            }


            var statusCode = context.Response.StatusCode;
            var message = statusCode == 201 ? "Created" : "Success";

            object responseObj;
            try
            {
                var jsonElement = JsonSerializer.Deserialize<object>(bodyText);
                responseObj = new ApiResponse<object>(statusCode, message, jsonElement);
            }
            catch
            {
                responseObj = new ApiResponse<string>(statusCode, message, bodyText);
            }

            context.Response.ContentType = "application/json";
            context.Response.Body = originalBody;
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                responseObj,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ));
        }
        catch (Exception ex)
        {
            // ⚠️ Không xử lý ở đây → để GlobalExceptionMiddleware bắt
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[FormatResponseMiddleware] Rethrow exception: {ex.Message}");
            Console.ResetColor();

            // Ném lỗi ra lại để middleware trước đó (GlobalException) xử lý
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
