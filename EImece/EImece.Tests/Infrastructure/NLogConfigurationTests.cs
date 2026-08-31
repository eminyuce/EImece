using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EImece.Tests.Infrastructure
{
    [TestClass]
    public class NLogConfigurationTests
    {
        private string GetNLogConfigPath()
        {
            string asmPath = typeof(NLogConfigurationTests).Assembly.Location;
            string dir = Path.GetDirectoryName(asmPath);
            while (!string.IsNullOrEmpty(dir))
            {
                string configCandidate1 = Path.Combine(dir, "NLog.config");
                if (File.Exists(configCandidate1))
                {
                    return configCandidate1;
                }
                string configCandidate2 = Path.Combine(dir, "EImece", "NLog.config");
                if (File.Exists(configCandidate2))
                {
                    return configCandidate2;
                }
                dir = Directory.GetParent(dir)?.FullName;
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NLog.config"));
        }

        [TestMethod]
        public void NLogConfig_FileExists_AndIsParsableXml()
        {
            var configPath = GetNLogConfigPath();
            Assert.IsTrue(File.Exists(configPath), $"NLog.config not found at: {configPath}");

            var doc = XDocument.Load(configPath);
            Assert.IsNotNull(doc.Root);
            Assert.AreEqual("nlog", doc.Root.Name.LocalName);
        }

        [TestMethod]
        public void NLogConfig_DatabaseTarget_IsAsynchronous_AndConfiguredWithDiscardOverflow()
        {
            var configPath = GetNLogConfigPath();
            var doc = XDocument.Load(configPath);

            var ns = doc.Root.Name.Namespace;

            // Check targets root has async="true" or explicit AsyncWrapper wrapping database
            var targetsElem = doc.Descendants(ns + "targets").FirstOrDefault()
                              ?? doc.Descendants("targets").FirstOrDefault();

            Assert.IsNotNull(targetsElem, "targets element must exist in NLog.config");

            var isTargetsAsync = string.Equals(targetsElem.Attribute("async")?.Value, "true", StringComparison.OrdinalIgnoreCase);

            var dbTarget = doc.Descendants().FirstOrDefault(e =>
                e.Name.LocalName == "target" &&
                e.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"))?.Value == "Database");

            Assert.IsNotNull(dbTarget, "Database target must exist in NLog.config");

            var parentWrapper = dbTarget.Parent;
            var isParentAsyncWrapper = parentWrapper != null &&
                parentWrapper.Name.LocalName == "target" &&
                parentWrapper.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"))?.Value == "AsyncWrapper";

            Assert.IsTrue(isTargetsAsync || isParentAsyncWrapper,
                "Database target must be asynchronous (either via targets async='true' or explicit AsyncWrapper target).");

            if (isParentAsyncWrapper)
            {
                var overflowAction = parentWrapper.Attribute("overflowAction")?.Value;
                Assert.AreEqual("Discard", overflowAction, "AsyncWrapper for database must use overflowAction='Discard' to prevent blocking HTTP request threads.");
            }
        }

        [TestMethod]
        public void NLogConfig_Rules_RouteWarnAndError_ToAsynchronousDatabase()
        {
            var configPath = GetNLogConfigPath();
            var doc = XDocument.Load(configPath);

            var rules = doc.Descendants().Where(e => e.Name.LocalName == "logger").ToList();
            Assert.IsTrue(rules.Count > 0, "NLog.config must define logger rules");

            var dbRule = rules.FirstOrDefault(r =>
            {
                var writeTo = r.Attribute("writeTo")?.Value ?? "";
                return writeTo.Contains("database") || writeTo.Contains("asyncDatabase");
            });

            Assert.IsNotNull(dbRule, "Must have a rule routing logs to database / asyncDatabase target.");
            var minLevel = dbRule.Attribute("minlevel")?.Value;
            Assert.IsTrue(string.Equals(minLevel, "Warn", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(minLevel, "Error", StringComparison.OrdinalIgnoreCase),
                "Database logging rule should capture Warn/Error level logs.");
        }

        [TestMethod]
        public void NLogConfig_CanBeLoadedAndInitializedByNLogFactory()
        {
            var configPath = GetNLogConfigPath();
            Assert.IsTrue(File.Exists(configPath), $"NLog.config not found at: {configPath}");

            // Load and parse through NLog's configuration loader
            var config = new NLog.Config.XmlLoggingConfiguration(configPath);
            Assert.IsNotNull(config, "XmlLoggingConfiguration should initialize cleanly without throwing exceptions.");
            Assert.IsTrue(config.AllTargets.Count > 0, "NLog configuration should register targets.");
        }

        [TestMethod]
        public void NLogConfig_DefaultLogsLocation_IsMediaLogs()
        {
            var configPath = GetNLogConfigPath();
            var doc = XDocument.Load(configPath);
            var logsLocation = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "variable" && e.Attribute("name")?.Value == "LogsLocation")
                ?.Attribute("value")?.Value;

            Assert.IsNotNull(logsLocation);
            StringAssert.Contains(logsLocation, "media/logs");
            StringAssert.DoesNotMatch(logsLocation, new System.Text.RegularExpressions.Regex("App_Data", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        }
    }
}
