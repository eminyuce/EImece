using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.AdminModels;

namespace EImece.Domain.Services.IServices
{
    public interface IProductService : IBaseContentService<Product>
    {
        List<Product> GetAdminPageList(int id, string search, int lang);
        List<Product> GetMainPageProducts(int page, int lang);
        List<ProductTag> GetProductTagsByProductId(int productId);
        void SaveProductTags(int id, int[] tags);
        ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId);
        Product GetProductById(int id);
        void DeleteProductById(int id);
    }
}
