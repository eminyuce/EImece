using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace EImece.Tests.Caching
{
    [TestClass]
    public class MemoryCacheDefaultUsageTests
    {
        [TestMethod]
        public void OnlyMemoryCacheProvider_MayUseRuntimeDefaultCacheHost()
        {
            var token = "MemoryCache" + "." + "Default";
            var root = FindSourceRoot();
            Assert.IsNotNull(root, "Could not locate source root from " + typeof(MemoryCacheDefaultUsageTests).Assembly.Location);

            var offenders = new List<string>();
            foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (ShouldSkip(path))
                {
                    continue;
                }

                var name = Path.GetFileName(path);
                if (string.Equals(name, "MemoryCacheProvider.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var text = File.ReadAllText(path);
                if (text.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    offenders.Add(path.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                }
            }

            Assert.AreEqual(
                0,
                offenders.Count,
                "Application code must use IEimeceCacheProvider. Offenders: " + string.Join("; ", offenders));
        }

        private static bool ShouldSkip(string path)
        {
            var normalized = path.Replace('/', '\\');
            return normalized.IndexOf("\\obj\\", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("\\bin\\", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("\\packages\\", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FindSourceRoot()
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(typeof(MemoryCacheDefaultUsageTests).Assembly.Location));
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "EImece.sln"))
                    || Directory.Exists(Path.Combine(dir.FullName, "EImece.Domain")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }
    }
}
