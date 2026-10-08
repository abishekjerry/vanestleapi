using System.Net;
using System.Text.Json;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        var response = new
        {
            Status = false,
            Message = "Server Error",
            Error = ex.InnerException?.Message ?? ex.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}