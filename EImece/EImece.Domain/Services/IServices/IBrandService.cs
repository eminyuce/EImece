using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBrandService : IBaseContentService<Brand>
    {
        List<Brand> GetAdminPageList(string search, int lang);

        Task<List<Brand>> GetAdminPageListAsync(string search, int lang);

        bool DeleteBrandById(int brandId);

        Brand GetBrandById(int BrandId);
        List<Brand> GetBrandsIfAnyProductExists(int lang);
    }
}