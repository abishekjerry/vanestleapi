using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using VAapiNestle.Configuration;

/// <summary>
/// Logs request bodies (method/path/status/timing are logged by Serilog request logging).
/// Credentials are redacted and bodies are truncated so logs never hold passwords or grow unbounded.
/// </summary>
public partial class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingSettings _settings;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IOptions<RequestLoggingSettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        if (_settings.LogBodies && context.Request.ContentLength > 0 && IsTextContent(context.Request.ContentType))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync(context.RequestAborted);
            context.Request.Body.Position = 0;

            body = SensitiveJsonValue().Replace(body, "\"$1\":\"***\"");
            if (body.Length > _settings.MaxBodyLength)
                body = body[.._settings.MaxBodyLength] + "...(truncated)";

            _logger.LogInformation("{Method} {Path} body: {Body}", context.Request.Method, context.Request.Path, body);
        }

        await _next(context);
    }

    private static bool IsTextContent(string? contentType) =>
        contentType != null &&
        (contentType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
         contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
         contentType.Contains("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase));

    // Matches "anything-containing-password/secret/token/key": "value" (works on malformed JSON too)
    [GeneratedRegex("\"([^\"]*(?:password|pwd|secret|token|apikey)[^\"]*)\"\\s*:\\s*\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveJsonValue();
}
