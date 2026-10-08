using System.Text;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        var body = "";
        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var log = $"{DateTime.Now} {context.Request.Method} {context.Request.Path} \n{body}\n";

        await File.AppendAllTextAsync("ApiLog.txt", log);

        await _next(context);
    }
}