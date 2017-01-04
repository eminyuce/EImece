using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;

namespace EImece.Domain.Repositories
{
    public class ProductTagRepository : BaseRepository<ProductTag>, IProductTagRepository
    {
        public ProductTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
 

        public List<ProductTag> GetAllByProductId(int productId)
        {
           return this.GetAll().Where(r => r.ProductId == productId).ToList();
        }
 
        public void DeleteProductTags(int productId)
        {
            var productTags = GetAll().Where(r => r.ProductId == productId).ToList();
            foreach (var product in productTags)
            {
                Delete(product);
            }
            Save();
        }
        public void SaveProductTags(int productId, int[] tags)
        {
            DeleteProductTags(productId);                
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    ProductTag item = new ProductTag();
                    item.ProductId = productId;
                    item.TagId = tag;
                    this.Add(item);
                }
                Save();

            }
         
        }
    }
}
