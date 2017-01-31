using EImece.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;


            routes.MapRoute(
                  name: "robots",
                  url: "robots.txt",
                          defaults: new
                          {
                              controller = "Robot",
                              action = "RobotsText"
                          }
               );

            routes.MapRoute(
                   name: "ImageResizing",
                   url: "images/{companyName}/{imageSize}/{id}",
                   defaults: new { controller = "images", action = Settings.ImageActionName, id = UrlParameter.Optional },
                   namespaces: new[] { "EImece.Controllers" }
               );

            routes.MapRoute(
                   name: "StoryTagPage",
                   url: "stories/tag/{id}",
                   defaults: new { controller = "stories", action = "Tag", id = UrlParameter.Optional },
                   namespaces: new[] { "EImece.Controllers" }
               );

            routes.MapRoute(
                   name: "StoryDetail",
                   url: "stories/{categoryName}/{id}",
                   defaults: new { controller = "stories", action = "Detail", id = UrlParameter.Optional },
                   namespaces: new[] { "EImece.Controllers" }
               );

            routes.MapRoute(
                  name: "PageDetail",
                  url: "pages/{id}",
                  defaults: new { controller = "Pages", action = "Detail", id = UrlParameter.Optional },
                  namespaces: new[] { "EImece.Controllers" }
              );
           
            routes.MapRoute(
                  name: "ProductTagPage",
                  url: "products/tag/{id}",
                  defaults: new { controller = "Products", action = "Tag", id = UrlParameter.Optional },
                  namespaces: new[] { "EImece.Controllers" }
              );

            routes.MapRoute(
               name: "ProductDetail",
               url: "products/{categoryName}/{id}",
               defaults: new { controller = "Products", action = "Detail", id = UrlParameter.Optional },
               namespaces: new[] { "EImece.Controllers" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "EImece.Controllers" }
            );
        }
    }
}
