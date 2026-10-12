using System.ComponentModel.DataAnnotations;

namespace VAapiNestle.Configuration
{
    public class ApiSettings
    {
        public const string SectionName = "ApiSettings";

        [Required(AllowEmptyStrings = false)]
        public string ApiKey { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string SecretKey { get; set; } = "";
    }

    public class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string FromName { get; set; } = "";
        public List<string> RedirectHosts { get; set; } = [];
        public string RedirectTo { get; set; } = "";
    }

    public class RequestLoggingSettings
    {
        public const string SectionName = "RequestLogging";

        public bool LogBodies { get; set; } = true;
        public int MaxBodyLength { get; set; } = 4096;
    }

    public class DatabaseSettings
    {
        public const string SectionName = "Database";

        public int CommandTimeoutSeconds { get; set; } = 60;
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelaySeconds { get; set; } = 5;
        public int CompatibilityLevel { get; set; } = 140;
        /// <summary>Timeout for the long-running report procedures.</summary>
        public int ReportCommandTimeoutSeconds { get; set; } = 300;
    }

    public class LoginRateLimitSettings
    {
        public const string SectionName = "LoginRateLimit";
        public const string PolicyName = "login";

        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
    }
}
