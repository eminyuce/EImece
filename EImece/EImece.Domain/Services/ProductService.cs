using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;

namespace EImece.Domain.Services
{
    public class ProductService : IProductService
    {
        public bool DeleteEntity(Product entity)
        {
            throw new NotImplementedException();
        }

        public bool DeleteEntityByWhere(Func<Product, bool> whereLambda)
        {
            throw new NotImplementedException();
        }

        public T[] ExecuteStoreQuery<T>(string commandText, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Product> LoadEntites(Func<Product, bool> whereLambda)
        {
            throw new NotImplementedException();
        }

        public Product SaveOrEditEntity(Product entity)
        {
            throw new NotImplementedException();
        }
    }
}
