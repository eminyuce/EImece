using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Domain.Helpers
{
    public class SeoUrlHelper
    {
        public static string GetCanonicalTag(ViewContext viewContext, string SeoId = "", string linkArea = "")
        {
            String url = GetCanonicalUrl(viewContext, SeoId, linkArea);
            if (!String.IsNullOrEmpty(url))
            {
                string canonicalRel = "canonical";
                if (linkArea.Equals("amp", StringComparison.InvariantCultureIgnoreCase))
                {
                    canonicalRel = "amphtml";
                }

                url = String.Format("<link href='{0}' rel='{1}'/>", url, canonicalRel);
                return url;
            }
            else
            {
                return String.Empty;
            }
        }

        public static string GetCanonicalUrl(ViewContext viewContext, string id = "", string linkArea = "")
        {
            string action = viewContext.Controller.ValueProvider.GetValue("action").RawValue.ToStr();
            string controller = viewContext.Controller.ValueProvider.GetValue("controller").RawValue.ToStr();
            bool isErrorPage = controller.Equals("Error", StringComparison.InvariantCultureIgnoreCase) &&
                               action.Equals("Index", StringComparison.InvariantCultureIgnoreCase);

            if (isErrorPage)
            {
                return "";
            }

            bool isImagePage = controller.Equals("Images", StringComparison.InvariantCultureIgnoreCase);

            if (isImagePage)
            {
                return "";
            }

            if (linkArea.Equals("amp", StringComparison.InvariantCultureIgnoreCase)
                && !AmpCanonical.IsRightAmpPages(action, controller))
            {
                // Not all page we want to create amp canonical links.
                return String.Empty;
            }

            if (string.IsNullOrEmpty(id))
            {
                try
                {
                    var valueProviderResult = viewContext.Controller.ValueProvider.GetValue("id");
                    if (valueProviderResult != null)
                    {
                        id = valueProviderResult.RawValue.ToStr();
                    }
                }
                catch
                {
                }
            }

            string domain = "";

            var uh = new UrlHelper(viewContext.RequestContext);

            String url = uh.Action(action, controller, new RouteValueDictionary(new { id = id, area = linkArea }), AppConfig.HttpProtocol, domain);
            // url = String.Format("<link href='{0}' rel='canonical'/>", url);
            return url;
        }
    }

    public class AmpCanonical
    {
        public String actionName { get; set; }
        public String controllerName { get; set; }

        private static List<AmpCanonical> GetAmpCanonicalPage()
        {
            var p = new List<AmpCanonical>();
            p.Add(new AmpCanonical() { actionName = "Detail", controllerName = "Stories" });
            p.Add(new AmpCanonical() { actionName = "Detail", controllerName = "Products" });
            return p;
        }

        public static bool IsRightAmpPages(string action, string controller)
        {
            var pages = GetAmpCanonicalPage();
            return pages.Any(t => t.controllerName.Equals(controller, StringComparison.InvariantCultureIgnoreCase) &&
                t.actionName.Equals(action, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}