using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagRepository : IBaseEntityRepository<Tag>
    {
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