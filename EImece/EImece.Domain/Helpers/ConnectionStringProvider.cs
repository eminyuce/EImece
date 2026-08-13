using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Resolves the SQL Server connection string without hard-coded credentials.
    /// Priority: environment variable EIMECE_DB_CONNECTION_STRING, then configuration.
    /// Fails closed when the value is missing or still a placeholder.
    /// </summary>
    public static class ConnectionStringProvider
    {
        public const string EnvironmentVariableName = "EIMECE_DB_CONNECTION_STRING";

        private static readonly string[] PlaceholderMarkers =
        {
            "YOUR_SERVER",
            "YOUR_DATABASE",
            "YOUR_USER",
            "YOUR_PASSWORD",
            "CHANGEME",
            "REPLACE_ME"
        };

        private static bool _initialized;
        private static readonly object InitLock = new object();

        /// <summary>
        /// Applies environment overrides into <see cref="ConfigurationManager"/> and validates.
        /// Call once as early as possible during application/test startup (before DbContext use).
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            lock (InitLock)
            {
                if (_initialized)
                {
                    return;
                }

                var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
                if (!string.IsNullOrWhiteSpace(fromEnvironment))
                {
                    SetConfigurationConnectionString(Constants.DbConnectionKey, fromEnvironment.Trim());
                }

                // Force validation so misconfiguration fails at startup, not on first request.
                GetConnectionString();
                _initialized = true;
            }
        }

        /// <summary>
        /// Returns the resolved connection string, or throws if missing/placeholder.
        /// </summary>
        public static string GetConnectionString(string name = null)
        {
            var connectionName = string.IsNullOrWhiteSpace(name) ? Constants.DbConnectionKey : name;

            var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(fromEnvironment) &&
                string.Equals(connectionName, Constants.DbConnectionKey, StringComparison.OrdinalIgnoreCase))
            {
                return Validate(fromEnvironment.Trim(), connectionName);
            }

            var settings = ConfigurationManager.ConnectionStrings[connectionName];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Database connection string '" + connectionName + "' is missing. " +
                    "Set environment variable '" + EnvironmentVariableName + "' to the full connection string, " +
                    "or configure it via ConnectionStrings.config (see ConnectionStrings.config.example). " +
                    "Do not commit real credentials to source control.");
            }

            return Validate(settings.ConnectionString.Trim(), connectionName);
        }

        /// <summary>
        /// Returns true when a usable (non-placeholder) connection string is available.
        /// </summary>
        public static bool TryGetConnectionString(out string connectionString, string name = null)
        {
            try
            {
                connectionString = GetConnectionString(name);
                return true;
            }
            catch (ConfigurationErrorsException)
            {
                connectionString = null;
                return false;
            }
        }

        public static string Validate(string connectionString, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException(
                    "Database connection string '" + connectionName + "' is empty. " +
                    "Set '" + EnvironmentVariableName + "' or provide a real connection string in configuration.");
            }

            foreach (var marker in PlaceholderMarkers.Where(m =>
                connectionString.IndexOf(m, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                throw new ConfigurationErrorsException(
                    "Database connection string '" + connectionName + "' still contains placeholder value '" + marker + "'. " +
                    "Replace placeholders with real values via environment variable '" + EnvironmentVariableName + "' " +
                    "or a gitignored ConnectionStrings.config. See docs/SECURE_CONNECTION_STRINGS.md.");
            }

            return connectionString;
        }

        /// <summary>
        /// Patches ConfigurationManager so EF / Identity name-based lookups pick up the env override.
        /// </summary>
        private static void SetConfigurationConnectionString(string name, string connectionString)
        {
            var settings = ConfigurationManager.ConnectionStrings[name];
            if (settings == null)
            {
                throw new ConfigurationErrorsException(
                    "Connection string entry '" + name + "' was not found in configuration. " +
                    "Ensure Web.config / App.config declares <add name=\"" + name + "\" ... /> " +
                    "(placeholder value is fine; the environment variable supplies the real secret).");
            }

            // NonPublic is required: ConfigurationElement._bReadOnly is a private field that must be cleared after config load.
#pragma warning disable S3011 // Must clear private ConfigurationElement._bReadOnly after loading connection strings
            var readOnlyField = typeof(ConfigurationElement).GetField(
                "_bReadOnly",
                BindingFlags.Instance | BindingFlags.NonPublic);
#pragma warning restore S3011
            if (readOnlyField != null)
            {
                readOnlyField.SetValue(settings, false);
            }

            settings.ConnectionString = connectionString;
        }
    }
}
