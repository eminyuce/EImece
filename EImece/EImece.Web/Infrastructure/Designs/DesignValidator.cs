using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EImece.Web.Infrastructure.Designs
{
    public class DesignValidationResult
    {
        public string Design { get; set; }
        public bool IsValid => MissingViews == null || MissingViews.Count == 0;
        public List<string> MissingViews { get; set; } = new List<string>();
        public int TotalRequiredViews { get; set; }
    }

    public static class DesignValidator
    {
        private const string ViewsFolder = "Views";

        public static DesignValidationResult ValidateDesign(string designName, string baseDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(designName))
            {
                return new DesignValidationResult { Design = designName, TotalRequiredViews = 0 };
            }

            string appRoot = ResolveAppRoot(baseDirectory);
            List<string> requiredRelativePaths = CollectRequiredViewPaths(appRoot);
            string designRoot = Path.GetFullPath(Path.Combine(appRoot, ViewsFolder, "Designs", designName));
            List<string> missingViews = FindMissingDesignViews(designRoot, requiredRelativePaths);

            return new DesignValidationResult
            {
                Design = designName,
                TotalRequiredViews = requiredRelativePaths.Count,
                MissingViews = missingViews.OrderBy(x => x).ToList()
            };
        }

        public static void EnsureValidDesign(string designName, string baseDirectory = null)
        {
            var result = ValidateDesign(designName, baseDirectory);
            if (!result.IsValid)
            {
                throw new DesignValidationException(designName, result.MissingViews);
            }
        }

        private static string ResolveAppRoot(string baseDirectory)
        {
            string appRoot = Path.GetFullPath(baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory);

            if (Directory.Exists(Path.Combine(appRoot, ViewsFolder)))
            {
                return appRoot;
            }

            string dir = appRoot;
            while (!string.IsNullOrEmpty(dir))
            {
                if (IsProjectViewsRoot(dir))
                {
                    return dir;
                }

                string subDir = Path.Combine(dir, "EImece");
                if (IsProjectViewsRoot(subDir))
                {
                    return subDir;
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            return appRoot;
        }

        private static bool IsProjectViewsRoot(string directory)
        {
            return File.Exists(Path.Combine(directory, "EImece.csproj"))
                && Directory.Exists(Path.Combine(directory, ViewsFolder));
        }

        private static List<string> CollectRequiredViewPaths(string appRoot)
        {
            string viewsRoot = Path.GetFullPath(Path.Combine(appRoot, ViewsFolder));
            string customersViewsRoot = Path.GetFullPath(Path.Combine(appRoot, "Areas", "Customers", ViewsFolder));
            List<string> requiredRelativePaths = new List<string>();

            AddRootViewPaths(viewsRoot, requiredRelativePaths);
            AddCustomerViewPaths(customersViewsRoot, requiredRelativePaths);
            return requiredRelativePaths;
        }

        private static void AddRootViewPaths(string viewsRoot, List<string> requiredRelativePaths)
        {
            if (!Directory.Exists(viewsRoot))
            {
                return;
            }

            var files = Directory.GetFiles(viewsRoot, "*.cshtml", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string relative = file.Substring(viewsRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (!IsExcludedDefaultView(relative))
                {
                    requiredRelativePaths.Add(relative);
                }
            }
        }

        private static void AddCustomerViewPaths(string customersViewsRoot, List<string> requiredRelativePaths)
        {
            if (!Directory.Exists(customersViewsRoot))
            {
                return;
            }

            var files = Directory.GetFiles(customersViewsRoot, "*.cshtml", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string relative = file.Substring(customersViewsRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                requiredRelativePaths.Add(Path.Combine("Areas", "Customers", relative));
            }
        }

        private static bool IsExcludedDefaultView(string relative)
        {
            return relative.StartsWith("Designs", StringComparison.OrdinalIgnoreCase) ||
                   relative.StartsWith(@"Shared\Griddly", StringComparison.OrdinalIgnoreCase) ||
                   relative.StartsWith("Shared/Griddly", StringComparison.OrdinalIgnoreCase) ||
                   relative.Equals(@"Shared\_ErrorLayout.cshtml", StringComparison.OrdinalIgnoreCase) ||
                   relative.Equals(@"Shared/_ErrorLayout.cshtml", StringComparison.OrdinalIgnoreCase) ||
                   relative.Equals(@"Account\AdminLogin.cshtml", StringComparison.OrdinalIgnoreCase) ||
                   relative.Equals(@"Account\AdminLockout.cshtml", StringComparison.OrdinalIgnoreCase) ||
                   relative.Equals(@"Account\VerifyAuthenticator.cshtml", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> FindMissingDesignViews(string designRoot, List<string> requiredRelativePaths)
        {
            List<string> missingViews = new List<string>();
            foreach (var relativePath in requiredRelativePaths)
            {
                string targetDesignViewPath = Path.Combine(designRoot, relativePath);
                if (!File.Exists(targetDesignViewPath))
                {
                    // Convert path separators to forward slash for clean output
                    missingViews.Add(relativePath.Replace('\\', '/'));
                }
            }

            return missingViews;
        }
    }
}
