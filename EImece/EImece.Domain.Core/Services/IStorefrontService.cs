using EImece.Domain.Core.Entities;

namespace EImece.Domain.Core.Services;

public interface IStorefrontService
{
    Task<IReadOnlyList<MainPageImage>> GetMainPageBannersAsync(int lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetHomeProductsAsync(int lang, int take, CancellationToken cancellationToken = default);
    Task<Product?> GetProductDetailAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsByTagAsync(int tagId, int lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchProductsAsync(string? query, int? categoryId, int lang, int take, CancellationToken cancellationToken = default);
    Task<Tag?> GetTagAsync(int tagId, CancellationToken cancellationToken = default);
    Task<Story?> GetStoryDetailAsync(int storyId, CancellationToken cancellationToken = default);
    Task<StoryCategory?> GetStoryCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Story>> GetStoriesByCategoryAsync(int categoryId, int lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Story>> GetStoriesByTagAsync(int tagId, int lang, CancellationToken cancellationToken = default);
    Task<Menu?> GetMenuPageAsync(int menuId, CancellationToken cancellationToken = default);
    Task<Menu?> GetMenuPageByLinkAsync(string menuLink, int lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Loc, DateTime? LastMod)>> GetSitemapUrlsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsForRssAsync(int take, int lang, int? categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Story>> GetStoriesForRssAsync(int take, int lang, int? categoryId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string userId, CancellationToken cancellationToken = default);
}
