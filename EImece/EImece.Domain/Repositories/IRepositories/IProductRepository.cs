using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductRepository : IBaseRepository<Product, int>, IDisposable
    {
        PaginatedList<Product> GetMainPageProducts(int page);
        int SaveOrEdit(Product p);
        int DeleteItem(Product p);

        List<Product> GetAdminPageList(int categoryId, string search);
    }
}
