using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests
{
    [TestClass]
    public class TestAssemblyInitializer
    {
        static TestAssemblyInitializer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var requestedName = new AssemblyName(args.Name).Name;
                var loaded = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in loaded)
                {
                    if (string.Equals(asm.GetName().Name, requestedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return asm;
                    }
                }

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var candidate = Path.Combine(baseDir, requestedName + ".dll");
                if (File.Exists(candidate))
                {
                    return Assembly.LoadFrom(candidate);
                }
            }
            catch
            {
                // ignore resolution error
            }

            return null;
        }
    }
}
