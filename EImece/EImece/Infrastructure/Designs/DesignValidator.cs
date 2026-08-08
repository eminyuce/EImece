using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EImece.Infrastructure.Designs
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
        public static DesignValidationResult ValidateDesign(string designName, string baseDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(designName))
            {
                return new DesignValidationResult { Design = designName, TotalRequiredViews = 0 };
            }

            string appRoot = Path.GetFullPath(baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory);

            if (!Directory.Exists(Path.Combine(appRoot, "Views")))
            {
                string dir = appRoot;
                while (!string.IsNullOrEmpty(dir))
                {
                    if (File.Exists(Path.Combine(dir, "EImece.csproj")) && Directory.Exists(Path.Combine(dir, "Views")))
                    {
                        appRoot = dir;
                        break;
                    }
                    string subDir = Path.Combine(dir, "EImece");
                    if (File.Exists(Path.Combine(subDir, "EImece.csproj")) && Directory.Exists(Path.Combine(subDir, "Views")))
                    {
                        appRoot = subDir;
                        break;
                    }
                    dir = Directory.GetParent(dir)?.FullName;
                }
            }

            string viewsRoot = Path.GetFullPath(Path.Combine(appRoot, "Views"));
            string customersViewsRoot = Path.GetFullPath(Path.Combine(appRoot, "Areas", "Customers", "Views"));
            string designRoot = Path.GetFullPath(Path.Combine(appRoot, "Views", "Designs", designName));

            List<string> requiredRelativePaths = new List<string>();

            // Scan root Views/
            if (Directory.Exists(viewsRoot))
            {
                var files = Directory.GetFiles(viewsRoot, "*.cshtml", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string relative = file.Substring(viewsRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    
                    // Skip Designs directory and AdminLogin
                    if (relative.StartsWith("Designs", StringComparison.OrdinalIgnoreCase) ||
                        relative.Equals(@"Account\AdminLogin.cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    requiredRelativePaths.Add(relative);
                }
            }

            // Scan Areas/Customers/Views/
            if (Directory.Exists(customersViewsRoot))
            {
                var files = Directory.GetFiles(customersViewsRoot, "*.cshtml", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string relative = file.Substring(customersViewsRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    requiredRelativePaths.Add(Path.Combine("Areas", "Customers", relative));
                }
            }

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
    }
}
