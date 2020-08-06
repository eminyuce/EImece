using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;

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

        public bool DeleteBrandById(int brandId)
        {
            return BrandRepository.DeleteByWhereCondition(r => r.Id == brandId);
        }

        public Brand GetBrandById(int brandId)
        {
            return BrandRepository.GetSingle(brandId);
        }
    }
}