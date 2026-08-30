using System;
using System.Collections.Generic;

namespace EImece.Web.Infrastructure.Designs
{
    public class DesignValidationException : Exception
    {
        public string Design { get; }
        public IReadOnlyList<string> MissingViews { get; }

        public DesignValidationException(string design, IReadOnlyList<string> missingViews)
            : base(BuildErrorMessage(design, missingViews))
        {
            Design = design;
            MissingViews = missingViews ?? new List<string>().AsReadOnly();
        }

        private static string BuildErrorMessage(string design, IReadOnlyList<string> missingViews)
        {
            return $"Design validation failed.\n\n" +
                   $"Design: {design}\n\n" +
                   $"Missing views:\n" +
                   $"- {string.Join("\n- ", missingViews ?? new string[0])}\n\n" +
                   $"The selected design '{design}' is incomplete. Please add all required views to the design folder before deploying to production.";
        }
    }
}
