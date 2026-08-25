using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    /// <summary>
    /// Read-only data access for the compressed-image export feature.
    /// </summary>
    public interface IImageExportRepository
    {
        Task<List<FileStorage>> GetFileStoragesAsync(CancellationToken cancellationToken);

        Task<List<ProductImageInfo>> GetProductImageInfosAsync(CancellationToken cancellationToken);

        Task<List<ProductFileImageInfo>> GetProductFileImageInfosAsync(CancellationToken cancellationToken);

        Task<List<CategoryImageInfo>> GetProductCategoryImageInfosAsync(CancellationToken cancellationToken);

        Task<List<MenuImageInfo>> GetMenuImageInfosAsync(CancellationToken cancellationToken);

        Task<List<MenuFileImageInfo>> GetMenuFileImageInfosAsync(CancellationToken cancellationToken);

        Task<List<StoryImageInfo>> GetStoryImageInfosAsync(CancellationToken cancellationToken);

        Task<List<StoryFileImageInfo>> GetStoryFileImageInfosAsync(CancellationToken cancellationToken);

        Task<List<CategoryImageInfo>> GetStoryCategoryImageInfosAsync(CancellationToken cancellationToken);

        Task<List<BrandImageInfo>> GetBrandImageInfosAsync(CancellationToken cancellationToken);
    }
}
