using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Ninject;

namespace EImece.Domain.Services
{
    public class ProductService : BaseContentService<Product>, IProductService
    {
          [Inject]
        public IProductRepository ProductRepository { get; set; }
     
 
        public override void SetCurrentRepository()
        {
            this.baseRepository = ProductRepository;
        }

        public List<Product> GetAdminPageList(int id, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(id, search, lang);
        }

        public List<Product> GetMainPageProducts(int page, int lang)
        {
            return ProductRepository.GetMainPageProducts(page, lang);
        }
    }
}
