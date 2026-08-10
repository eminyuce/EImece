using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IStoryCategoryService : IBaseContentService<StoryCategory>
    {
        void DeleteStoryCategoryById(int storyCategoryId);

        Task DeleteStoryCategoryByIdAsync(int storyCategoryId);

        List<StoryCategory> GetActiveStoryCategories(int language);

        Task<List<StoryCategory>> GetActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}