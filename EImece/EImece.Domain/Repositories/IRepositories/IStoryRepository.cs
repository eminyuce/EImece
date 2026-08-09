using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryRepository : IBaseContentRepository<Story>
    {
        List<Story> GetAdminPageList(int categoryId, string search, int lang);

        Task<List<Story>> GetAdminPageListAsync(int categoryId, string search, int lang);

        Story GetStoryById(int storyId);

        Task<Story> GetStoryByIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<Story> GetMainPageStories(int page, int pageSize, int language);

        Task<PaginatedList<Story>> GetMainPageStoriesAsync(int page, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<Story> GetRelatedStories(int[] tagIdList, int take, int lang, int excludedStoryId);

        Task<List<Story>> GetRelatedStoriesAsync(int[] tagIdList, int take, int lang, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken));

        PaginatedList<Story> GetStoriesByStoryCategoryId(int storyCategoryId, int language, int pageIndex, int pageSize);

        Task<PaginatedList<Story>> GetStoriesByStoryCategoryIdAsync(int storyCategoryId, int language, int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken));

        List<Story> GetLatestStories(int language, int take);

        List<Story> GetFeaturedStories(int take, int language, int excludedStoryId);

        Task<List<Story>> GetFeaturedStoriesAsync(int take, int language, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken));

        Story GetPreviousStory(int currentStoryId, int language);

        Task<Story> GetPreviousStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken));

        Story GetNextStory(int currentStoryId, int language);

        Task<Story> GetNextStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken));

    }
}