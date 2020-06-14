using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.ServiceModel.Syndication;

namespace EImece.Domain.Services.IServices
{
    public interface IStoryService : IBaseContentService<Story>
    {
        List<Story> GetAdminPageList(int categoryId, string search, int lang);

        List<StoryTag> GetStoryTagsByStoryId(int storyId);

        void DeleteStoryById(int storyId);

        Story GetStoryById(int storyId);

        StoryIndexViewModel GetMainPageStories(int page, int currentLanguage);

        void SaveStoryTags(int storyId, int[] tags);

        StoryDetailViewModel GetStoryDetailViewModel(int storyId);

        StoryCategoryViewModel GetStoryCategoriesViewModel(int storyCategoryId, int page);

        List<Story> GetLatestStories(int language, int take);

        Rss20FeedFormatter GetStoryCategoriesRss(RssParams rssParams);

        SimiliarStoryTagsViewModel GetStoriesByTagId(int tagId, int pageIndex,
            int pageSize, int currentLanguage);

        Rss20FeedFormatter GetStoryCategoriesRssFull(RssParams rssParams);
    }
}