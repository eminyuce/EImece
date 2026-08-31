using Microsoft.Extensions.Logging;
using System;
using System.Configuration;

namespace EImece.Domain.Observability.Logging
{
    /// <summary>
    /// Classic ASP.NET logging configuration from Web.config / App.config appSettings.
    /// Default file path is <c>media/logs</c> (same writable root as uploads; HTTP denied via media/Web.config).
    /// </summary>
    public sealed class LoggingOptions
    {
        public const string DefaultFileRelativePath = "media/logs";

        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        public bool FileEnabled { get; set; } = true;

        /// <summary>Relative path under the web app base directory. Default: media/logs.</summary>
        public string FilePath { get; set; } = DefaultFileRelativePath;

        public string RollingInterval { get; set; } = "Day";

        public int RetentionArchiveCount { get; set; } = 10;

        public long ArchiveAboveSizeBytes { get; set; } = 5_000_000;

        public bool ConsoleEnabled { get; set; }

        public bool DatabaseEnabled { get; set; } = true;

        public string ApplicationName { get; set; } = "EImece";

        public string Environment { get; set; }

        public int AsyncQueueLimit { get; set; } = 10_000;

        public static LoggingOptions FromAppConfig()
        {
            return Cached.Value;
        }

        internal static void ResetForTests()
        {
            Cached = new Lazy<LoggingOptions>(BuildFromAppConfig);
        }

        private static Lazy<LoggingOptions> Cached = new Lazy<LoggingOptions>(BuildFromAppConfig);

        private static LoggingOptions BuildFromAppConfig()
        {
            var minimumLevel = ParseLogLevel(
                AppConfig.GetConfigString("Logging:MinimumLevel", "Information"),
                LogLevel.Information);

            var filePath = AppConfig.GetConfigString("Logging:File:Path", DefaultFileRelativePath);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = DefaultFileRelativePath;
            }

            filePath = filePath.Trim().TrimStart('~', '/').Replace('/', System.IO.Path.DirectorySeparatorChar);

            return new LoggingOptions
            {
                MinimumLevel = minimumLevel,
                FileEnabled = AppConfig.GetConfigBool("Logging:File:Enabled", true),
                FilePath = filePath,
                RollingInterval = AppConfig.GetConfigString("Logging:File:RollingInterval", "Day"),
                RetentionArchiveCount = AppConfig.GetConfigInt("Logging:File:Retention", 10),
                ArchiveAboveSizeBytes = AppConfig.GetConfigInt("Logging:File:ArchiveAboveSizeBytes", 5_000_000),
                ConsoleEnabled = AppConfig.GetConfigBool("Logging:Console:Enabled", false),
                DatabaseEnabled = AppConfig.GetConfigBool("Logging:Database:Enabled", true),
                ApplicationName = AppConfig.GetConfigString("Logging:ApplicationName", "EImece"),
                Environment = AppConfig.GetConfigString("Logging:Environment", AppConfig.GetConfigString("SiteStatus", "dev")),
                AsyncQueueLimit = AppConfig.GetConfigInt("Logging:File:AsyncQueueLimit", 10_000),
            };
        }

        public string ResolveAbsoluteLogDirectory()
        {
            var relative = (FilePath ?? DefaultFileRelativePath).Trim().TrimStart('~', '/').Replace('/', System.IO.Path.DirectorySeparatorChar);
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
        }

        private static LogLevel ParseLogLevel(string value, LogLevel fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (Enum.TryParse(value, true, out LogLevel parsed))
            {
                return parsed;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "trace": return LogLevel.Trace;
                case "debug": return LogLevel.Debug;
                case "info":
                case "information": return LogLevel.Information;
                case "warn":
                case "warning": return LogLevel.Warning;
                case "error": return LogLevel.Error;
                case "fatal":
                case "critical": return LogLevel.Critical;
                default: return fallback;
            }
        }
    }
}
