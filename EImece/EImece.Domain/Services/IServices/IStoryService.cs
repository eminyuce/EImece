using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  
    }
}
