using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IStoryCategoryService : IBaseContentService<StoryCategory>
    {
        void DeleteStoryCategoryById(int storyCategoryId);

        List<StoryCategory> GetActiveStoryCategories(int language);
    }
}