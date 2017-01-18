using EImece.Domain.Entities;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{

    public interface IProductCategoryRepository : IBaseContentRepository<ProductCategory>, IDisposable
    {
        List<ProductCategory> BuildTree(bool ? isActive, int language=1);
        List<Node> CreateProductCategoryTreeViewDataList();
        ProductCategory GetProductCategory(int categoryId);
        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);
    }
}
