using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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
            var brand = BrandRepository.GetSingle(brandId);
            if (brand == null)
            {
                return false;
            }

            if (brand.MainImageId.HasValue && brand.MainImageId.Value > 0)
            {
                FileStorageService.DeleteFileStorage(brand.MainImageId.Value);
            }

            return BrandRepository.DeleteByWhereCondition(r => r.Id == brandId);
        }

        public async Task<bool> DeleteBrandByIdAsync(int brandId)
        {
            var brand = await BrandRepository.GetSingleAsync(brandId).ConfigureAwait(false);
            if (brand == null)
            {
                return false;
            }

            if (brand.MainImageId.HasValue && brand.MainImageId.Value > 0)
            {
                await FileStorageService.DeleteFileStorageAsync(brand.MainImageId.Value).ConfigureAwait(false);
            }

            return await BrandRepository.DeleteByWhereConditionAsync(r => r.Id == brandId).ConfigureAwait(false);
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var v in values)
            {
                DeleteBrandById(v.ToInt());
            }
        }

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var v in values)
            {
                await DeleteBrandByIdAsync(v.ToInt()).ConfigureAwait(false);
            }
        }

        public Brand GetBrandById(int brandId)
        {
            return BrandRepository.GetSingle(brandId);
        }

        public List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0)
        {
            return BrandRepository.GetBrandsIfAnyProductExists(lang, categoryId);
        }

        public async Task<List<Brand>> GetBrandsIfAnyProductExistsAsync(int lang, int categoryId = 0)
        {
            return await BrandRepository.GetBrandsIfAnyProductExistsAsync(lang, categoryId).ConfigureAwait(false);
        }
    }
}