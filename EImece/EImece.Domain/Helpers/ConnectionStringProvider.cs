using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Resolves the SQL Server connection string without hard-coded credentials.
    /// Priority: environment variable EIMECE_DB_CONNECTION_STRING,
    /// then ConnectionStrings.config one folder above the site (outside publish),
    /// then Web.config / App.config.
    /// Fails closed when the value is missing or still a placeholder.
    /// </summary>
    public static class ConnectionStringProvider
    {
        public const string EnvironmentVariableName = "EIMECE_DB_CONNECTION_STRING";
        public const string ParentConfigFileName = "ConnectionStrings.config";

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
        /// Applies environment / parent-folder overrides into <see cref="ConfigurationManager"/> and validates.
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
                else
                {
                    TryApplyParentConfigFile(Constants.DbConnectionKey);
                }

                // Force validation so misconfiguration fails at startup, not on first request.
                GetConnectionString();
                _initialized = true;
            }
        }

        /// <summary>
        /// Site folder parent + ConnectionStrings.config, e.g. C:\inetpub\wwwroot\ConnectionStrings.config
        /// when the app is C:\inetpub\wwwroot\Eimece.
        /// </summary>
        public static string GetParentConnectionStringsPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                return null;
            }

            var appDir = baseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parent = Directory.GetParent(appDir);
            if (parent == null)
            {
                return null;
            }

            return Path.Combine(parent.FullName, ParentConfigFileName);
        }

        /// <summary>
        /// Reads a named connection string from a configSource-style XML file.
        /// Root may be connectionStrings, or configuration/connectionStrings.
        /// </summary>
        public static string TryReadNamedConnectionStringFromFile(string path, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            var doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(
                    "Could not read connection strings file '" + path + "'. " + ex.Message,
                    ex);
            }

            var root = doc.DocumentElement;
            if (root == null)
            {
                return null;
            }

            XmlNodeList adds;
            if (string.Equals(root.Name, "connectionStrings", StringComparison.OrdinalIgnoreCase))
            {
                adds = root.SelectNodes("add");
            }
            else
            {
                adds = root.SelectNodes("connectionStrings/add");
            }

            if (adds == null)
            {
                return null;
            }

            foreach (XmlNode node in adds)
            {
                var attrs = node.Attributes;
                if (attrs == null)
                {
                    continue;
                }

                var name = attrs["name"] != null ? attrs["name"].Value : null;
                if (!string.Equals(name, connectionName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = attrs["connectionString"] != null ? attrs["connectionString"].Value : null;
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }

            return null;
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
            var fromConfig = settings != null ? settings.ConnectionString : null;
            if (!string.IsNullOrWhiteSpace(fromConfig) && !ContainsPlaceholder(fromConfig))
            {
                return fromConfig.Trim();
            }

            var fromParent = TryApplyParentConfigFile(connectionName);
            if (!string.IsNullOrWhiteSpace(fromParent))
            {
                return Validate(fromParent, connectionName);
            }

            if (settings == null || string.IsNullOrWhiteSpace(fromConfig))
            {
                throw new ConfigurationErrorsException(
                    "Database connection string '" + connectionName + "' is missing. " +
                    "Set environment variable '" + EnvironmentVariableName + "', " +
                    "or place " + ParentConfigFileName + " one folder above the site " +
                    "(for IIS: C:\\inetpub\\wwwroot\\" + ParentConfigFileName + "), " +
                    "or configure Web.config. Do not commit real credentials to source control.");
            }

            return Validate(fromConfig.Trim(), connectionName);
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

            var marker = FindPlaceholder(connectionString);
            if (marker != null)
            {
                var parentPath = GetParentConnectionStringsPath();
                throw new ConfigurationErrorsException(
                    "Database connection string '" + connectionName + "' still contains placeholder value '" + marker + "'. " +
                    "Replace placeholders via environment variable '" + EnvironmentVariableName + "' " +
                    "or " + ParentConfigFileName + " one folder above the site" +
                    (string.IsNullOrEmpty(parentPath) ? "" : " (" + parentPath + ")") +
                    ". See docs/SECURE_CONNECTION_STRINGS.md.");
            }

            return connectionString;
        }

        private static bool ContainsPlaceholder(string connectionString)
        {
            return FindPlaceholder(connectionString) != null;
        }

        private static string FindPlaceholder(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return null;
            }

            return PlaceholderMarkers.FirstOrDefault(m =>
                connectionString.IndexOf(m, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string TryApplyParentConfigFile(string connectionName)
        {
            var path = GetParentConnectionStringsPath();
            var value = TryReadNamedConnectionStringFromFile(path, connectionName);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            SetConfigurationConnectionString(connectionName, value);
            return value;
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
                    "(placeholder value is fine; the environment variable or parent " +
                    ParentConfigFileName + " supplies the real secret).");
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
