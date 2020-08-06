using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IBrandService : IBaseContentService<Brand>
    {
        List<Brand> GetAdminPageList(string search, int lang);

        bool DeleteBrandById(int brandId);

        Brand GetBrandById(int BrandId);
    }
}