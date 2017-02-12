using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
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
        List<ProductCategoryTreeModel> BuildTree(bool ? isActive, int language=1);
        List<Node> CreateProductCategoryTreeViewDataList();
        ProductCategory GetProductCategory(int categoryId);
        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);
        List<ProductCategory> GetMainPageProductCategories(int language);
    }
}
