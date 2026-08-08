using System;

namespace EImece.Infrastructure.Designs
{
    public class MissingDesignViewException : Exception
    {
        public string Design { get; }
        public string Controller { get; }
        public string Action { get; }
        public string ViewName { get; }
        public string ExpectedPath { get; }
        public string Area { get; }
        public string[] SearchedLocations { get; }

        public MissingDesignViewException(
            string design,
            string controller,
            string action,
            string viewName,
            string expectedPath,
            string area = null,
            string[] searchedLocations = null)
            : base(BuildErrorMessage(design, controller, action, viewName, expectedPath, area))
        {
            Design = design;
            Controller = controller;
            Action = action;
            ViewName = viewName;
            ExpectedPath = expectedPath;
            Area = area ?? "None";
            SearchedLocations = searchedLocations ?? Array.Empty<string>();
        }

        private static string BuildErrorMessage(
            string design,
            string controller,
            string action,
            string viewName,
            string expectedPath,
            string area)
        {
            return $"Design View Not Found\n\n" +
                   $"Active Design:\n{design}\n\n" +
                   $"Controller:\n{controller}\n\n" +
                   $"Action:\n{action}\n\n" +
                   $"Requested View:\n{viewName}\n\n" +
                   $"Expected View:\n{expectedPath}\n\n" +
                   $"Area:\n{area ?? "None"}\n\n" +
                   $"The selected design '{design}' does not contain this required view.\n" +
                   $"Please add the missing Razor view to the selected design before deploying to production.";
        }
    }
}
