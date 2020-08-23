using EImece.Domain.Entities;
using GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryRepository : IBaseContentRepository<Story>
    {
        List<Story> GetAdminPageList(int categoryId, string search, int lang);

        Story GetStoryById(int storyId);

        PaginatedList<Story> GetMainPageStories(int page, int pageSize, int language);

        List<Story> GetRelatedStories(int[] tagIdList, int take, int lang, int excludedStoryId);

        PaginatedList<Story> GetStoriesByStoryCategoryId(int storyCategoryId, int language, int pageIndex, int pageSize);

        List<Story> GetLatestStories(int language, int take);

        List<Story> GetFeaturedStories(int take, int language, int storyId);
    }
}