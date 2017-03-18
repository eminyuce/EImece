using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductRepository : IBaseContentRepository<Product>, IDisposable
    {
        PaginatedList<Product> GetActiveProducts(int pageIndex, int pageSize, int language);
        PaginatedList<Product> GetMainPageProducts(int pageIndex, int pageSize, int language);
        List<Product> GetAdminPageList(int categoryId, string search, int language);
        Product GetProduct(int id);
        PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang);
        IEnumerable<Product> GetData(out int totalRecords, string globalSearch, String name, int? limitOffset, int? limitRowCount, string orderBy, bool desc);
        List<Product> GetRelatedProducts(int[] tagIdList, int take, int lang, int excludedProductId);
        IQueryable<Product> GetActiveProducts(bool? isActive, int? language);
    }
}
