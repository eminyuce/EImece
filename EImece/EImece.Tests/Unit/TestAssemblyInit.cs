using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Unit
{
    [TestClass]
    public class TestAssemblyInit
    {
        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name);
                if (!string.Equals(name.Name, "System.Threading.Tasks.Extensions", StringComparison.OrdinalIgnoreCase))
                    return null;

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(baseDir, "System.Threading.Tasks.Extensions.dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }
    }
}
