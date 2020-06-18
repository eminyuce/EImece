using System.Web.Mvc;

namespace EImece.Areas.Amp
{
    public class AmpAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Amp";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                       name: "AmpStoryDetail",
                       url: "amp/stories/{categoryName}/{id}",
                       defaults: new { controller = "stories", action = "Detail", id = UrlParameter.Optional },
                       namespaces: new[] { "EImece.Areas.Amp.Controllers" }
                   );

            context.MapRoute(
                 name: "AmpProductDetail",
                 url: "amp/products/{categoryName}/{id}",
                 defaults: new { controller = "Products", action = "Detail", id = UrlParameter.Optional },
                 namespaces: new[] { "EImece.Areas.Amp.Controllers" }
             );

            context.MapRoute(
                "Amp_default",
                "Amp/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                , namespaces: new[] { "EImece.Areas.Amp.Controllers" }
            );
        }
    }
}