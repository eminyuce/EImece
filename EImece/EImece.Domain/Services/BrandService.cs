using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class BrandService : BaseContentService<Brand>, IBrandService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IBrandRepository BrandRepository;

        public BrandService(IBrandRepository repository) : base(repository)
        {
            BrandRepository = repository;
        }

        public List<Brand> GetAdminPageList(string search, int lang)
        {
            return BrandRepository.GetAdminPageList(search, lang);
        }

        public async Task<List<Brand>> GetAdminPageListAsync(string search, int lang)
        {
            return await BrandRepository.GetAdminPageListAsync(search, lang).ConfigureAwait(false);
        }

        public bool DeleteBrandById(int brandId)
        {
            return BrandRepository.DeleteByWhereCondition(r => r.Id == brandId);
        }

        public Brand GetBrandById(int brandId)
        {
            return BrandRepository.GetSingle(brandId);
        }

        public List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0)
        {
            return BrandRepository.GetBrandsIfAnyProductExists(lang, categoryId);
        }
    }
}