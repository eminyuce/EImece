using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using System;
using System.IO;

namespace EImece.Domain.Observability.Logging
{
    /// <summary>
    /// Composition-root logging setup: MEL is the application contract; NLog is the async file/DB sink bridge.
    /// Default log directory: media/logs (configurable via Logging:File:Path).
    /// </summary>
    public static class LoggingBootstrap
    {
        private static ILoggerFactory _factory;
        private static bool _initialized;

        public static ILoggerFactory LoggerFactory => _factory;

        public static ILoggerFactory Configure()
        {
            return Configure(LoggingOptions.FromAppConfig());
        }

        public static ILoggerFactory Configure(LoggingOptions options)
        {
            if (_initialized && _factory != null)
            {
                return _factory;
            }

            options = options ?? LoggingOptions.FromAppConfig();
            ConfigureNLogTargets(options);

            var builder = Microsoft.Extensions.Logging.LoggerFactory.Create(logging =>
            {
                logging.SetMinimumLevel(options.MinimumLevel);
                logging.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                logging.AddFilter("LuckyPennySoftware.AutoMapper.License", Microsoft.Extensions.Logging.LogLevel.Warning);

                if (options.FileEnabled || options.DatabaseEnabled)
                {
                    logging.AddProvider(new FailSafeLoggerProvider(new NLogLoggerProvider()));
                }

                if (options.ConsoleEnabled)
                {
                    logging.AddConsole(consoleOptions =>
                    {
                        consoleOptions.IncludeScopes = true;
                        consoleOptions.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                    });
                }
            });

            _factory = builder;
            _initialized = true;
            return _factory;
        }

        private static void ConfigureNLogTargets(LoggingOptions options)
        {
            var config = LogManager.Configuration ?? new LoggingConfiguration();

            var logDirectory = options.ResolveAbsoluteLogDirectory();
            Directory.CreateDirectory(logDirectory);

            config.Variables["LogsLocation"] = logDirectory;
            config.Variables["ApplicationName"] = options.ApplicationName ?? "EImece";
            config.Variables["EnvironmentName"] = options.Environment ?? "dev";

            SetTargetEnabled(config, "asyncDatabase", options.DatabaseEnabled);
            SetTargetEnabled(config, "asyncJsonFile", options.FileEnabled);
            SetTargetEnabled(config, "asyncFlatFile", options.FileEnabled);
            SetTargetEnabled(config, "asyncEfSqlFile", options.FileEnabled);

            ApplyAsyncQueueLimit(config, "asyncDatabase", options.AsyncQueueLimit);
            ApplyAsyncQueueLimit(config, "asyncJsonFile", options.AsyncQueueLimit);
            ApplyAsyncQueueLimit(config, "asyncFlatFile", options.AsyncQueueLimit);
            ApplyAsyncQueueLimit(config, "asyncEfSqlFile", options.AsyncQueueLimit);

            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }

        private static void SetTargetEnabled(LoggingConfiguration config, string targetName, bool enabled)
        {
            foreach (var rule in config.LoggingRules)
            {
                if (!RuleWritesToTarget(rule, targetName))
                {
                    continue;
                }

                if (enabled)
                {
                    rule.EnableLoggingForLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                }
                else
                {
                    rule.DisableLoggingForLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                }
            }
        }

        private static bool RuleWritesToTarget(NLog.Config.LoggingRule rule, string targetName)
        {
            foreach (var target in rule.Targets)
            {
                if (string.Equals(target.Name, targetName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyAsyncQueueLimit(LoggingConfiguration config, string wrapperName, int queueLimit)
        {
            var target = config.FindTargetByName(wrapperName) as NLog.Targets.Wrappers.AsyncTargetWrapper;
            if (target != null && queueLimit > 0)
            {
                target.QueueLimit = queueLimit;
                target.OverflowAction = NLog.Targets.Wrappers.AsyncTargetWrapperOverflowAction.Discard;
            }
        }

        public static void FlushAndShutdown()
        {
            try
            {
                _factory?.Dispose();
            }
            catch
            {
                // Best-effort during AppDomain recycle.
            }
            finally
            {
                _factory = null;
                _initialized = false;
            }

            try
            {
                LogManager.Flush();
                LogManager.Shutdown();
            }
            catch
            {
                // Best-effort.
            }
        }
    }
}
