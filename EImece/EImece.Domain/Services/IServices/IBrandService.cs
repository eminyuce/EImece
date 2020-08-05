using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBrandService : IBaseContentService<Brand>
    {
        List<Brand> GetAdminPageList(string search, int lang);

        bool DeleteBrandById(int brandId);

        Brand GetBrandById(int BrandId);
    }
}
