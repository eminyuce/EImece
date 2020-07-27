using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryCategoryRepository : IBaseContentRepository<StoryCategory>
    {
        StoryCategory GetStoryCategoryById(int storyCategoryId);

        List<StoryCategory> GetActiveStoryCategories(int language);
    }
}