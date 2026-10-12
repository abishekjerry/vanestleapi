using System.Threading.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using VAapiNestle.Configuration;
using VAapiNestle.CustomModels;
using VAapiNestle.Models;
using VAapiNestle.Services;
using static VAapiNestle.CustomModels.Base;

var builder = WebApplication.CreateBuilder(args);

// Logging (rolling files under Logs/, replaces the old unbounded ApiLog.txt)
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

// Settings (validated at startup so a missing secret fails fast instead of on the first request)
builder.Services.AddOptions<ApiSettings>()
    .Bind(builder.Configuration.GetSection(ApiSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.Configure<RequestLoggingSettings>(builder.Configuration.GetSection(RequestLoggingSettings.SectionName));
var loginRateLimit = builder.Configuration.GetSection(LoginRateLimitSettings.SectionName).Get<LoginRateLimitSettings>() ?? new();

// Controllers + Validation
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<ApiKeyAuthFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new NullToEmptyStringConverter());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string[] headers = ["PMG-API-KEY", "PMG-Secret-KEY", "PMG-Account"];
    string[] schemeIds = ["ApiKey", "SecretKey", "Account"];
    for (var i = 0; i < headers.Length; i++)
    {
        options.AddSecurityDefinition(schemeIds[i], new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = headers[i],
            Type = SecuritySchemeType.ApiKey
        });
    }
    options.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement();
        foreach (var id in schemeIds)
            requirement.Add(new OpenApiSecuritySchemeReference(id, document), []);
        return requirement;
    });
});

// DB
var baseConnection = builder.Configuration.GetConnectionString("VADbConnectionString");
if (string.IsNullOrEmpty(baseConnection))
    throw new InvalidOperationException(
        "VADbConnectionString is missing. Set it with 'dotnet user-secrets' (development) or the " +
        "ConnectionStrings__VADbConnectionString environment variable (server).");
var dbSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new();

// Transient failures - including SQL Server deadlock victims (error 1205) and dropped connections - are retried
// automatically. Explicit transactions run inside the execution strategy (see VAController.InTransactionAsync).
void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString) =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(dbSettings.MaxRetryCount, TimeSpan.FromSeconds(dbSettings.MaxRetryDelaySeconds), null);
        sql.CommandTimeout(dbSettings.CommandTimeoutSeconds);
        // The database runs at compatibility level 140 (SQL Server 2017): never generate newer T-SQL
        sql.UseCompatibilityLevel(dbSettings.CompatibilityLevel);
    });

builder.Services.AddDbContext<AppDbContext>(options => ConfigureSqlServer(options, baseConnection));
var masterBuilder = new SqlConnectionStringBuilder(baseConnection)
{
    InitialCatalog = "Master"
};
builder.Services.AddDbContext<MasterDbContext>(options => ConfigureSqlServer(options, masterBuilder.ConnectionString));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.Configure<PaginationSettings>(builder.Configuration.GetSection(PaginationSettings.SectionName));

var allowedOrigins = builder.Configuration.GetSection("HostURLS").Get<List<string>>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCorsPolicy", policy =>
    {
        policy.WithOrigins([.. allowedOrigins]) // Use WithOrigins instead of AllowAnyOrigin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders([.. Pagination.Headers, "Content-Disposition"]); // let the browser read paging totals
    });
});

// Smaller payloads for the large JSON lists (dashboard, history search)
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

// Brute-force protection for the login endpoint (per client IP)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(LoginRateLimitSettings.PolicyName, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginRateLimit.PermitLimit,
                Window = TimeSpan.FromSeconds(loginRateLimit.WindowSeconds),
                QueueLimit = 0
            }));
});

// Services
builder.Services.AddScoped<IEmailService, EmailService>();

// Filters
builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

var app = builder.Build();

// Middleware (error handler first so failures anywhere below are caught and logged)
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseResponseCompression();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLoggingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AppCorsPolicy");
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

// Liveness (unchanged contract) and readiness (checks the database connection)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();
