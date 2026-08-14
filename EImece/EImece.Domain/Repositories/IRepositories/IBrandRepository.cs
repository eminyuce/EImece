using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBrandRepository : IBaseContentRepository<Brand>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<List<StorefrontBrandDto>> GetStorefrontBrandsAsync(int lang, int categoryId = 0, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontBrandDto> GetStorefrontBrands(int lang, int categoryId = 0);
        Task<StorefrontBrandDto> GetStorefrontBrandByIdAsync(int brandId, CancellationToken cancellationToken = default(CancellationToken));
        StorefrontBrandDto GetStorefrontBrandById(int brandId);

        #endregion

        List<Brand> GetAdminPageList(string search, int lang);

        Task<List<Brand>> GetAdminPageListAsync(string search, int lang);

        List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0);

        Task<List<Brand>> GetBrandsIfAnyProductExistsAsync(int lang, int categoryId = 0);
    }
}