using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBrandRepository : IBaseContentRepository<Brand>
    {
        List<Brand> GetAdminPageList(string search, int lang);

        Task<List<Brand>> GetAdminPageListAsync(string search, int lang);

        List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0);

        Task<List<Brand>> GetBrandsIfAnyProductExistsAsync(int lang, int categoryId = 0);
    }
}