using NLog;
using Serilog;
using System.Data.Entity;

namespace EImece.Domain.Observability.Logging
{
    /// <summary>
    /// Routes Entity Framework 6 <see cref="Database.Log"/> output into NLog and Serilog.
    /// </summary>
    public static class EfSqlLogger
    {
        public const string LoggerName = "EntityFramework.Sql";

        private static readonly Logger NLogLogger = LogManager.GetLogger(LoggerName);
        private static readonly object Sync = new object();
        private static bool _enabled;

        public static bool IsEnabled
        {
            get { return _enabled; }
        }

        public static void Configure(bool enabled)
        {
            lock (Sync)
            {
                _enabled = enabled;
            }
        }

        public static void Attach(DbContext context)
        {
            if (!_enabled || context == null)
            {
                return;
            }

            context.Database.Log = Write;
        }

        public static void Write(string sql)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            var message = SensitiveDataMasker.Mask(sql.TrimEnd());
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            NLogLogger.Debug(message);
            Log.ForContext("SourceContext", LoggerName)
                .Debug("{Sql}", message);
        }
    }
}
