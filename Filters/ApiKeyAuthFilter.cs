using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IConfiguration _config;

    public ApiKeyAuthFilter(IConfiguration config)
    {
        _config = config;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        // ✅ Skip Swagger
        if (request.Path.StartsWithSegments("/swagger"))
            return;

        var apiKey = request.Headers["PMG-API-KEY"].FirstOrDefault();
        var secretKey = request.Headers["PMG-Secret-KEY"].FirstOrDefault();

        var validApiKey = _config["ApiSettings:ApiKey"];
        var validSecretKey = _config["ApiSettings:SecretKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != validApiKey)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
            return;
        }

        if (string.IsNullOrEmpty(secretKey) || secretKey != validSecretKey)
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
}