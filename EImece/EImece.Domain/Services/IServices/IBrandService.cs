using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBrandService : IBaseContentService<Brand>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<List<StorefrontBrandDto>> GetStorefrontBrandsAsync(int lang, int categoryId = 0, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontBrandDto> GetStorefrontBrands(int lang, int categoryId = 0);
        Task<StorefrontBrandDto> GetStorefrontBrandByIdAsync(int brandId, CancellationToken cancellationToken = default(CancellationToken));
        StorefrontBrandDto GetStorefrontBrandById(int brandId);

        #endregion

        List<Brand> GetAdminPageList(string search, int lang);

        Task<List<Brand>> GetAdminPageListAsync(string search, int lang);

        bool DeleteBrandById(int brandId);

        Task<bool> DeleteBrandByIdAsync(int brandId);

        Brand GetBrandById(int BrandId);
        List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0);

        Task<List<Brand>> GetBrandsIfAnyProductExistsAsync(int lang, int categoryId = 0);
    }
}