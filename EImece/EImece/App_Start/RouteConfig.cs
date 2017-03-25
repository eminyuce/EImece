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

            // Imprive SEO by stopping duplicate URL's due to case or trailing slashes.
            routes.AppendTrailingSlash = true;
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
               name: "SitemapsIndex",
               url: "sitemap.xml",
                       defaults: new { controller = "SiteMap", action = "Index" }
            );
            routes.MapRoute(
              name: "sendcontactus",
              url: "home/sendContactUs",
              defaults: new { controller = "home", action = "SendContactUs" },
              namespaces: new[] { "EImece.Controllers" }
          );


            routes.MapRoute(
                 name: "privacypolicy",
                 url: "privacypolicy",
                 defaults: new { controller = "home", action = "privacypolicy" },
                 namespaces: new[] { "EImece.Controllers" }
             );


            routes.MapRoute(
                 name: "termsandconditions",
                 url: "termsandconditions",
                 defaults: new { controller = "home", action = "termsandconditions" },
                 namespaces: new[] { "EImece.Controllers" }
             );
         routes.MapRoute(
                   name: "getcaptcha",
                   url: "images/getcaptcha",
                   defaults: new { controller = "images", action = "getcaptcha" },
                   namespaces: new[] { "EImece.Controllers" }
               );
            routes.MapRoute(
                   name: "ImageResizing",
                   url: "images/{imageSize}/{id}",
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
                   name: "Storycategories",
                   url: "stories/categories/{id}",
                   defaults: new { controller = "stories", action = "categories", id = UrlParameter.Optional },
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
                 name: "SearchProducts",
                 url: "products/searchproducts",
                 defaults: new { controller = "Products", action = "searchproducts" },
                 namespaces: new[] { "EImece.Controllers" }
             );
            routes.MapRoute(
               name: "SearchProducts2",
               url: "products/advancedsearchproducts",
               defaults: new { controller = "Products", action = "advancedsearchproducts" },
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
