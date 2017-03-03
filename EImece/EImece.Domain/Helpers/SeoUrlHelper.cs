using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Domain.Helpers
{
    public class SeoUrlHelper
    {

        public static string GetCanonicalTag(ViewContext viewContext, string SeoId = "")
        {
            String url = GetCanonicalUrl(viewContext, SeoId);
            if (!String.IsNullOrEmpty(url))
            {
                url = String.Format("<link href='{0}' rel='canonical'/>", url);
                return url;
            }
            else
            {
                return String.Empty;
            }
        }


        public static string GetCanonicalUrl(ViewContext viewContext, string id = "")
        {
            string action = viewContext.Controller.ValueProvider.GetValue("action").RawValue.ToStr();
            string controller = viewContext.Controller.ValueProvider.GetValue("controller").RawValue.ToStr();
            bool isErrorPage = controller.Equals("Error", StringComparison.InvariantCultureIgnoreCase) &&
                               action.Equals("Index", StringComparison.InvariantCultureIgnoreCase);

            if (isErrorPage)
            {
                return "";
            }
            // string id = "";
            if (string.IsNullOrEmpty(id))
            {
                try
                {
                    id = viewContext.Controller.ValueProvider.GetValue("id").RawValue.ToStr();
                }
                catch
                {
                }
            }

            string domain =  "";


            var uh = new UrlHelper(viewContext.RequestContext);

            String url = uh.Action(action, controller, new RouteValueDictionary(new { id = id }), Settings.HttpProtocol, domain);
            // url = String.Format("<link href='{0}' rel='canonical'/>", url);
            return url;

        }

    }
}
