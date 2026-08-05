using EImece.Domain.Core.Entities;

namespace EImece.Web.Models;

internal static class StorefrontMapping
{
    public static ProductListItemViewModel ToListItem(Product product)
    {
        var categoryName = product.ProductCategory?.Name ?? "urun";
        return new ProductListItemViewModel
        {
            Id = product.Id,
            Name = product.Name,
            ProductCode = product.ProductCode,
            Price = product.Price,
            CategoryId = product.ProductCategoryId,
            CategoryName = categoryName,
            CategorySlug = Slug(categoryName)
        };
    }

    public static StoryListItemViewModel ToStoryListItem(Story story)
    {
        var categoryName = story.StoryCategory?.Name ?? "hikaye";
        return new StoryListItemViewModel
        {
            Id = story.Id,
            Name = story.Name,
            ShortDescription = story.ShortDescription,
            CategoryName = categoryName,
            CategorySlug = Slug(categoryName)
        };
    }

    public static string Slug(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "item";
        }

        var chars = name.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars).Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }
}
