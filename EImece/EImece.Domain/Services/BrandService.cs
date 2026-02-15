using AutoMapper;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Linq;

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

        public List<Brand> GetBrandsIfAnyProductExists(int lang)
        {
            return BrandRepository.GetBrandsIfAnyProductExists(lang);
        }
        
        public List<BrandDto> GetBrandsIfAnyProductExistsAsDtos(int lang)
        {
            var brands = BrandRepository.GetBrandsIfAnyProductExists(lang);
            return brands.Select(b => Mapper.Map<BrandDto>(b)).ToList();
        }
    }
}