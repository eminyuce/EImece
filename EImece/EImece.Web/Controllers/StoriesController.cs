using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class StoriesController : BaseController
{
    private readonly IStorefrontService _storefront;

    public StoriesController(IOptions<EImeceOptions> siteOptions, IStorefrontService storefront)
        : base(siteOptions)
    {
        _storefront = storefront;
    }

    public async Task<IActionResult> Detail(string categoryName, string? id, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(categoryName) ? "hikaye" : categoryName;
        var storyId = SeoIdParser.Parse(id);
        if (storyId <= 0)
        {
            return View(new StoryDetailViewModel { Name = "Hikaye", CategorySlug = slug, Notice = "Geçersiz hikaye kimliği." });
        }

        try
        {
            var story = await _storefront.GetStoryDetailAsync(storyId, cancellationToken).ConfigureAwait(false);
            if (story is null || !story.IsActive)
            {
                return View(new StoryDetailViewModel { Id = storyId, Name = $"Hikaye {storyId}", CategorySlug = slug, Notice = "Hikaye bulunamadı." });
            }

            var categoryNameResolved = story.StoryCategory?.Name ?? slug;
            return View(new StoryDetailViewModel
            {
                Id = story.Id,
                Name = story.Name,
                ShortDescription = story.ShortDescription,
                Description = story.Description,
                AuthorName = story.AuthorName,
                CategoryId = story.StoryCategoryId,
                CategoryName = categoryNameResolved,
                CategorySlug = StorefrontMapping.Slug(categoryNameResolved)
            });
        }
        catch (Exception ex)
        {
            return View(new StoryDetailViewModel { Id = storyId, Name = $"Hikaye {storyId}", CategorySlug = slug, Notice = ex.Message });
        }
    }

    public async Task<IActionResult> Tag(string? id, CancellationToken cancellationToken)
    {
        var tagId = SeoIdParser.Parse(id);
        var model = new StoryTagViewModel { TagId = tagId, TagName = $"Etiket {tagId}" };
        if (tagId <= 0)
        {
            return View(model);
        }

        try
        {
            var tag = await _storefront.GetTagAsync(tagId, cancellationToken).ConfigureAwait(false);
            if (tag is not null)
            {
                model.TagName = tag.Name;
            }

            var stories = await _storefront.GetStoriesByTagAsync(tagId, SiteOptions.MainLanguage, cancellationToken).ConfigureAwait(false);
            model.Stories = stories.Select(StorefrontMapping.ToStoryListItem).ToList();
        }
        catch
        {
            model.Stories = Array.Empty<StoryListItemViewModel>();
        }

        return View(model);
    }

    public async Task<IActionResult> Categories(string? id, CancellationToken cancellationToken)
    {
        var categoryId = SeoIdParser.Parse(id);
        if (categoryId <= 0)
        {
            return View(new StoryCategoryViewModel { Name = "Hikaye kategorileri", Notice = "Geçersiz kategori." });
        }

        try
        {
            var category = await _storefront.GetStoryCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            if (category is null)
            {
                return View(new StoryCategoryViewModel { Id = categoryId, Name = $"Kategori {categoryId}", Notice = "Kategori bulunamadı." });
            }

            var stories = await _storefront.GetStoriesByCategoryAsync(categoryId, SiteOptions.MainLanguage, cancellationToken).ConfigureAwait(false);
            return View(new StoryCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Stories = stories.Select(StorefrontMapping.ToStoryListItem).ToList()
            });
        }
        catch (Exception ex)
        {
            return View(new StoryCategoryViewModel { Id = categoryId, Name = $"Kategori {categoryId}", Notice = ex.Message });
        }
    }
}
