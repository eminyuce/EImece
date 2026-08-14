using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IStoryCategoryService : IBaseContentService<StoryCategory>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<List<StorefrontCategoryDto>> GetStorefrontActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontCategoryDto> GetStorefrontActiveStoryCategories(int language);
        Task<StorefrontCategoryDto> GetStorefrontStoryCategoryByIdAsync(int storyCategoryId, CancellationToken cancellationToken = default(CancellationToken));
        StorefrontCategoryDto GetStorefrontStoryCategoryById(int storyCategoryId);

        #endregion

        void DeleteStoryCategoryById(int storyCategoryId);

        Task DeleteStoryCategoryByIdAsync(int storyCategoryId);

        List<StoryCategory> GetActiveStoryCategories(int language);

        Task<List<StoryCategory>> GetActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}