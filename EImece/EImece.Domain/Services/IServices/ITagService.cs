using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ITagService : IBaseEntityService<Tag>
    {
        List<Tag> GetAdminPageList(String search, int language);

        Task<List<Tag>> GetAdminPageListAsync(String search, int language);

        void DeleteTagById(int tagId);

        Tag GetTagById(int tagId);
        List<Tag> GetProductTags(int language);

        /// <summary>
        /// Active tags linked to at least one product or story, with <see cref="Tag.ItemCount"/> populated.
        /// </summary>
        List<Tag> GetTagsWithEntityCounts(int language, int minEntityCount = 1);

        /// <summary>
        /// Active tags linked to at least one story (for /s/t/ pages). <see cref="Tag.ItemCount"/> is the story count.
        /// </summary>
        List<Tag> GetTagsWithStoryCounts(int language, int minStoryCount = 1);
    }
}