namespace EImece.Web.Infrastructure.Routing;

/// <summary>SEO route prefixes — parity with legacy EImece.Domain.Constants.</summary>
public static class RouteConstants
{
    public const string ProductsPrefix = "p";
    public const string CategoriesPrefix = "c";
    public const string StoriesPrefix = "s";
    public const string PagesPrefix = "i";
    public const string PaymentOrdersPrefix = "o";
    public const string SearchProductSegment = "arama";
    public const string ProductTagSegment = "t/{id}";
    public const string StoryTagSegment = "t/{id}";
    public const string CategorySegment = "pc/{id}";
    public const string CultureCookieName = "_culture";
    public const string AdminCultureCookieName = "_adminCulture";
    public const string LanguageCookieName = "Language";
}
