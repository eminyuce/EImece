using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryCategoryRepository : IBaseContentRepository<StoryCategory>
    {
        StoryCategory GetStoryCategoryById(int storyCategoryId);

        List<StoryCategory> GetActiveStoryCategories(int language);

        Task<List<StoryCategory>> GetActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}