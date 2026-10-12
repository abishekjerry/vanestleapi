using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using VAapiNestle.Configuration;

public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly ApiSettings _settings;

    public ApiKeyAuthFilter(IOptions<ApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        var apiKey = request.Headers["PMG-API-KEY"].FirstOrDefault();
        var secretKey = request.Headers["PMG-Secret-KEY"].FirstOrDefault();

        if (!FixedTimeEquals(apiKey, _settings.ApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
            return;
        }

        if (!FixedTimeEquals(secretKey, _settings.SecretKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid Secret Key");
            return;
        }

        var account = request.Headers["PMG-Account"].FirstOrDefault();
        if (string.IsNullOrEmpty(account))
        {
            context.Result = new UnauthorizedObjectResult("Invalid Account");
        }
    }

    // Constant-time comparison so key values cannot be discovered through response timing
    private static bool FixedTimeEquals(string? provided, string expected)
    {
        if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(expected))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }
}
