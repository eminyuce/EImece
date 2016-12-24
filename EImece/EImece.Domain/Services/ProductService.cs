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
    public class ProductService : BaseService<Product, int>, IProductService
    {
        [Inject]
        public IProductRepository ProductRepository { get; set; }
        public override void SetCurrentRepository()
        {
            this.baseRepository = ProductRepository;
        }
    }
}
