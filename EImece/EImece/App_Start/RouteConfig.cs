using EImece.Domain;
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
            routes.MapMvcAttributeRoutes(); //Enables Attribute Routing

            routes.MapRoute(
                  name: "robots",
                  url: "robots.txt",
                  defaults: new
                  {
                      controller = "Robot",
                      action = "RobotsText"
                  },
                    namespaces: new[] { Constants.ControllersNamespace }
               );

            routes.MapRoute(
                  name: "webappmanifest",
                  url: "manifest.json",
                  defaults: new
                  {
                      controller = "Manifest",
                      action = "Index"
                  },
                    namespaces: new[] { Constants.ControllersNamespace }
               );

            routes.MapRoute(
               name: "UnderConstruction",
               url: "UnderConstruction",
                       defaults: new { controller = "UnderConstruction", action = "Index" },
              namespaces: new[] { Constants.ControllersNamespace }
            );

            routes.MapRoute(
               name: "SitemapsIndex",
               url: "sitemap.xml",
                       defaults: new { controller = "SiteMap", action = "Index" },
              namespaces: new[] { Constants.ControllersNamespace }
            );
            routes.MapRoute(
                   name: "WebSiteGeneralInfoPages",
                   url: "info/{id}",
                   defaults: new { controller = "info", action = "index" },
                   namespaces: new[] { Constants.ControllersNamespace }
               );
            routes.MapRoute(
                     name: "Getlogo",
                     url: "images/logo.jpg",
                     defaults: new { controller = "images", action = "logo" },
                     namespaces: new[] { Constants.ControllersNamespace }
                 );

            routes.MapRoute(
                  name: "GetDefaultImage",
                  url: "images/defaultImage/{imageSize}/default.jpg",
                  defaults: new { controller = "images", action = "defaultImage" },
                  namespaces: new[] { Constants.ControllersNamespace }
              );

            routes.MapRoute(
                      name: "getcaptcha",
                      url: "images/getcaptcha",
                      defaults: new { controller = "images", action = "getcaptcha" },
                      namespaces: new[] { Constants.ControllersNamespace }
                  );

            routes.MapRoute(
                 name: "ImageResizing",
                 url: "images/{imageSize}/{id}",
                 defaults: new { controller = "images", action = Constants.ImageActionName, id = UrlParameter.Optional },
                 namespaces: new[] { Constants.ControllersNamespace }
             );

            routes.MapRoute(
                   name: "StoryTagPage",
                   url: Constants.StoriesCategoriesControllerRoutingPrefix + Constants.UrlPathSeparator + Constants.StoryTagPrefix,
                   defaults: new { controller = Constants.StoriesRoute, action = "Tag", id = UrlParameter.Optional },
                   namespaces: new[] { Constants.ControllersNamespace }
               );

            routes.MapRoute(
                   name: "Storycategories",
                   url: Constants.StoriesCategoriesControllerRoutingPrefix + Constants.UrlPathSeparator + Constants.StoryCategoryPrefix,
                   defaults: new { controller = Constants.StoriesRoute, action = "categories", id = UrlParameter.Optional },
                   namespaces: new[] { Constants.ControllersNamespace }
               );

            // SEO: keep old /s/categories/{id} URLs working via 301 → /s/sc/{id}
            routes.MapRoute(
                   name: "StorycategoriesLegacy",
                   url: Constants.StoriesCategoriesControllerRoutingPrefix + "/categories/{id}",
                   defaults: new { controller = Constants.StoriesRoute, action = "CategoriesLegacy", id = UrlParameter.Optional },
                   namespaces: new[] { Constants.ControllersNamespace }
               );

            routes.MapRoute(
                   name: "StoryDetail",
                   url: Constants.StoriesCategoriesControllerRoutingPrefix + "/{categoryName}/{id}",
                   defaults: new { controller = Constants.StoriesRoute, action = "Detail", id = UrlParameter.Optional },
                   namespaces: new[] { Constants.ControllersNamespace }
               );

            routes.MapRoute(
                  name: "PageDetail",
                  url: Constants.PagesControllerRoutingPrefix + "/{id}",
                  defaults: new { controller = "Pages", action = "Detail", id = UrlParameter.Optional },
                  namespaces: new[] { Constants.ControllersNamespace }
              );

            routes.MapRoute(
                  name: "ProductTagPage",
                  url: Constants.ProductsControllerRoutingPrefix + Constants.UrlPathSeparator + Constants.ProductTagPrefix,
                  defaults: new { controller = "Products", action = "Tag", id = UrlParameter.Optional },
                  namespaces: new[] { Constants.ControllersNamespace }
              );

            routes.MapRoute(
                 name: "SearchProducts",
                 url: Constants.ProductsControllerRoutingPrefix + Constants.UrlPathSeparator + Constants.SearchProductPrefix,
                 defaults: new { controller = "Products", action = "searchproducts" },
                 namespaces: new[] { Constants.ControllersNamespace }
             );

            routes.MapRoute(
               name: "SearchProducts2",
               url: Constants.ProductsControllerRoutingPrefix + "/advancedsearchproducts",
               defaults: new { controller = "Products", action = "advancedsearchproducts" },
               namespaces: new[] { Constants.ControllersNamespace }
           );

            routes.MapRoute(
               name: "PaymentBuyNow",
               url: "b/{categoryName}/{id}",
               defaults: new { controller = "Payment", action = "BuyNow", id = UrlParameter.Optional },
               namespaces: new[] { Constants.ControllersNamespace }
           );

            routes.MapRoute(
               name: "ProductDetail",
               url: Constants.ProductsControllerRoutingPrefix + "/{categoryName}/{id}",
               defaults: new { controller = "Products", action = "Detail", id = UrlParameter.Optional },
               namespaces: new[] { Constants.ControllersNamespace }
           );

            // SEO: conventional MVC category URLs 301 → canonical /c/pc/{id}
            routes.MapRoute(
                name: "ProductCategoriesLegacyMvc",
                url: "productcategories/category/{id}",
                defaults: new { controller = "ProductCategories", action = "CategoryLegacyMvc", id = UrlParameter.Optional },
                namespaces: new[] { Constants.ControllersNamespace }
            );

            // SEO: conventional /stories/categories/{id} 301 → /s/sc/{id}
            routes.MapRoute(
                name: "StoryCategoriesLegacyMvc",
                url: "stories/categories/{id}",
                defaults: new { controller = Constants.StoriesRoute, action = "CategoriesLegacy", id = UrlParameter.Optional },
                namespaces: new[] { Constants.ControllersNamespace }
            );

            // SEO: canonical Stories root /s/ mapped to StoriesController.Index
            routes.MapRoute(
                name: "StoriesRoot",
                url: Constants.StoriesCategoriesControllerRoutingPrefix,
                defaults: new { controller = Constants.StoriesRoute, action = "Index" },
                namespaces: new[] { Constants.ControllersNamespace }
            );

            // Metrics route redirect to Admin area to prevent MissingDesignViewException
            var metricsRoute = routes.MapRoute(
                name: "MetricsRedirect",
                url: "metrics",
                defaults: new { controller = "Metrics", action = "Index" },
                namespaces: new[] { "EImece.Areas.Admin.Controllers" }
            );
            metricsRoute.DataTokens["area"] = "Admin";
            metricsRoute.DataTokens["UseNamespaceFallback"] = false;

            var defaultRoute = routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { Constants.ControllersNamespace }
            );
            defaultRoute.DataTokens["UseNamespaceFallback"] = false;
        }
    }
}