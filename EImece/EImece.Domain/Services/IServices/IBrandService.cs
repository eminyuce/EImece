using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IBrandService : IBaseContentService<Brand>
    {
        List<Brand> GetAdminPageList(string search, int lang);

        bool DeleteBrandById(int brandId);

        Brand GetBrandById(int BrandId);
        List<Brand> GetBrandsIfAnyProductExists(int lang);
        
        List<BrandDto> GetBrandsIfAnyProductExistsAsDtos(int lang);
    }
}