using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IProductService : IBaseContentService<Product>
    {
        List<Product> GetAdminPageList(int id, string search, int lang);
        List<Product> GetMainPageProducts(int page, int lang);
        List<ProductTag> GetProductTagsByProductId(int productId);
        void SaveProductTags(int id, int[] tags);
    }
}
