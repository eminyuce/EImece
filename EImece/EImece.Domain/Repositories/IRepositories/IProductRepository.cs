using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductRepository : IBaseRepository<Product, int>, IDisposable
    {
        List<Product> GetMainPageProducts(int? take, int? skip);
        int SaveOrEdit(Product p);
        int DeleteItem(Product p);

        List<Product> GetAdminPageList(int categoryId, string search);
    }
}
