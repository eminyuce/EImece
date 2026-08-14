using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagRepository : IBaseEntityRepository<Tag>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<List<StorefrontTagDto>> GetStorefrontProductTagsAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontTagDto> GetStorefrontProductTags(int language);
        Task<List<StorefrontTagDto>> GetStorefrontTagsWithStoryCountsAsync(int language, int minStoryCount = 1, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontTagDto> GetStorefrontTagsWithStoryCounts(int language, int minStoryCount = 1);
        Task<List<StorefrontTagDto>> GetStorefrontTagsWithEntityCountsAsync(int language, int minEntityCount = 1, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontTagDto> GetStorefrontTagsWithEntityCounts(int language, int minEntityCount = 1);
        Task<StorefrontTagDto> GetStorefrontTagByIdAsync(int tagId, CancellationToken cancellationToken = default(CancellationToken));
        StorefrontTagDto GetStorefrontTagById(int tagId);

        #endregion
        List<Tag> GetAdminPageList(String search, int language);

        Task<List<Tag>> GetAdminPageListAsync(String search, int language);

        Tag GetTagById(int tagId);
        List<Tag> GetProductTags(int language);

        /// <summary>
        /// Active tags that have at least <paramref name="minEntityCount"/> active product or story links.
        /// Sets <see cref="Tag.ItemCount"/> on each result.
        /// </summary>
        List<Tag> GetTagsWithEntityCounts(int language, int minEntityCount = 1);

        /// <summary>
        /// Active tags that have at least <paramref name="minStoryCount"/> active stories.
        /// Sets <see cref="Tag.ItemCount"/> to the story count (product links are ignored).
        /// </summary>
        List<Tag> GetTagsWithStoryCounts(int language, int minStoryCount = 1);

        Task<List<Tag>> GetTagsWithStoryCountsAsync(int language, int minStoryCount = 1, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Tag>> GetProductTagsAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}