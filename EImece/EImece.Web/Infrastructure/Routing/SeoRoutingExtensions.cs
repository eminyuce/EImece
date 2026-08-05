namespace EImece.Web.Infrastructure.Routing;

public static class SeoRoutingExtensions
{
    /// <summary>
    /// Registers legacy RouteConfig SEO routes (product/story/page/sitemap/robots/images).
    /// Call after area routes and before the default {controller}/{action} route.
    /// </summary>
    public static IEndpointRouteBuilder MapEImeceSeoRoutes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllerRoute(
            name: "robots",
            pattern: "robots.txt",
            defaults: new { controller = "Robot", action = "RobotsText" });

        endpoints.MapControllerRoute(
            name: "UnderConstruction",
            pattern: "UnderConstruction",
            defaults: new { controller = "UnderConstruction", action = "Index" });

        endpoints.MapControllerRoute(
            name: "SitemapsIndex",
            pattern: "sitemap.xml",
            defaults: new { controller = "SiteMap", action = "Index" });

        endpoints.MapControllerRoute(
            name: "WebSiteGeneralInfoPages",
            pattern: "info/{id?}",
            defaults: new { controller = "Info", action = "Index" });

        endpoints.MapControllerRoute(
            name: "Getlogo",
            pattern: "images/logo.jpg",
            defaults: new { controller = "Images", action = "Logo" });

        endpoints.MapControllerRoute(
            name: "GetDefaultImage",
            pattern: "images/defaultImage/{imageSize}/default.jpg",
            defaults: new { controller = "Images", action = "DefaultImage" });

        endpoints.MapControllerRoute(
            name: "getcaptcha",
            pattern: "images/getcaptcha",
            defaults: new { controller = "Images", action = "GetCaptcha" });

        endpoints.MapControllerRoute(
            name: "ImageResizing",
            pattern: "images/{imageSize}/{id?}",
            defaults: new { controller = "Images", action = "Index" });

        endpoints.MapControllerRoute(
            name: "StoryTagPage",
            pattern: $"{RouteConstants.StoriesPrefix}/t/{{id?}}",
            defaults: new { controller = "Stories", action = "Tag" });

        endpoints.MapControllerRoute(
            name: "Storycategories",
            pattern: $"{RouteConstants.StoriesPrefix}/categories/{{id?}}",
            defaults: new { controller = "Stories", action = "Categories" });

        endpoints.MapControllerRoute(
            name: "StoryDetail",
            pattern: $"{RouteConstants.StoriesPrefix}/{{categoryName}}/{{id?}}",
            defaults: new { controller = "Stories", action = "Detail" });

        endpoints.MapControllerRoute(
            name: "PageDetail",
            pattern: $"{RouteConstants.PagesPrefix}/{{id?}}",
            defaults: new { controller = "Pages", action = "Detail" });

        endpoints.MapControllerRoute(
            name: "ProductTagPage",
            pattern: $"{RouteConstants.ProductsPrefix}/t/{{id?}}",
            defaults: new { controller = "Products", action = "Tag" });

        endpoints.MapControllerRoute(
            name: "SearchProducts",
            pattern: $"{RouteConstants.ProductsPrefix}/{RouteConstants.SearchProductSegment}",
            defaults: new { controller = "Products", action = "SearchProducts" });

        endpoints.MapControllerRoute(
            name: "SearchProducts2",
            pattern: $"{RouteConstants.ProductsPrefix}/advancedsearchproducts",
            defaults: new { controller = "Products", action = "AdvancedSearchProducts" });

        endpoints.MapControllerRoute(
            name: "CategoryDetail",
            pattern: $"{RouteConstants.CategoriesPrefix}/pc/{{id?}}",
            defaults: new { controller = "ProductCategories", action = "Index" });

        endpoints.MapControllerRoute(
            name: "PaymentBuyNow",
            pattern: "b/{categoryName}/{id?}",
            defaults: new { controller = "Payment", action = "BuyNow" });

        endpoints.MapControllerRoute(
            name: "ProductDetail",
            pattern: $"{RouteConstants.ProductsPrefix}/{{categoryName}}/{{id?}}",
            defaults: new { controller = "Products", action = "Detail" });

        return endpoints;
    }
}
