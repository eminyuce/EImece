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
    public class ProductSpecificationRepository : BaseEntityRepository<ProductSpecification>, IProductSpecificationRepository
    {
        public ProductSpecificationRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

     
      
    }
}
