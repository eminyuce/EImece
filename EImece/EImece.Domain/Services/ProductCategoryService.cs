using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ProductCategoryService : BaseContentService<ProductCategory>, IProductCategoryService
    {
        private IProductCategoryRepository ProductCategoryRepository { get; set; }
        public ProductCategoryService(IProductCategoryRepository repository):base()
        {
            ProductCategoryRepository = repository;
            this.baseContentRepository = repository;
        }
    }
}
