using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IStoryService : IBaseContentService<Story>
    {
        List<Story> GetAdminPageList(int categoryId, string search, int lang);

        Task<List<Story>> GetAdminPageListAsync(int categoryId, string search, int lang);

        List<StoryTag> GetStoryTagsByStoryId(int storyId);

        Task<List<StoryTag>> GetStoryTagsByStoryIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken));

        void DeleteStoryById(int storyId);

        Task DeleteStoryByIdAsync(int storyId);

        Story GetStoryById(int storyId);

        Task<Story> GetStoryByIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken));

        StoryIndexViewModel GetMainPageStories(int page, int currentLanguage);

        Task<StoryIndexViewModel> GetMainPageStoriesAsync(int page, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken));

        void SaveStoryTags(int storyId, int[] tags);

        Task SaveStoryTagsAsync(int storyId, int[] tags);

        StoryDetailViewModel GetStoryDetailViewModel(int storyId);

        Task<StoryDetailViewModel> GetStoryDetailViewModelAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken));

        StoryCategoryViewModel GetStoryCategoriesViewModel(int storyCategoryId, int page);

        Task<StoryCategoryViewModel> GetStoryCategoriesViewModelAsync(int storyCategoryId, int page, CancellationToken cancellationToken = default(CancellationToken));

        List<Story> GetLatestStories(int language, int take);

        Rss20FeedFormatter GetStoryCategoriesRss(RssParams rssParams);

        Task<Rss20FeedFormatter> GetStoryCategoriesRssAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken));

        SimiliarStoryTagsViewModel GetStoriesByTagId(int tagId, int pageIndex,
            int pageSize, int currentLanguage);

        Task<SimiliarStoryTagsViewModel> GetStoriesByTagIdAsync(int tagId, int pageIndex,
            int pageSize, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken));

        Rss20FeedFormatter GetStoryCategoriesRssFull(RssParams rssParams);

        Task<Rss20FeedFormatter> GetStoryCategoriesRssFullAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken));

        List<Story> GetFeaturedStories(int take, int language, int storyId);

        Task<List<Story>> GetFeaturedStoriesAsync(int take, int language, int storyId, CancellationToken cancellationToken = default(CancellationToken));

        Story GetPreviousStory(int currentStoryId, int language);

        Story GetNextStory(int currentStoryId, int language);
    }
}