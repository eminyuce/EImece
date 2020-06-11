using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryCategoryRepository : IBaseContentRepository<StoryCategory>, IDisposable
    {
        StoryCategory GetStoryCategoryById(int storyCategoryId);
        List<StoryCategory> GetActiveStoryCategories(int language);
    }
}
