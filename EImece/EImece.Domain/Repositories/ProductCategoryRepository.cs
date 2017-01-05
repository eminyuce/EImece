using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;
using SharkDev.Web.Controls.TreeView.Model;
using EImece.Domain.Helpers;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class ProductCategoryRepository : BaseContentRepository<ProductCategory>, IProductCategoryRepository
    {
        public ProductCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        //Recursion method for recursively get all child nodes
        private void GetTreeview(List<ProductCategory> list, ProductCategory current, ref List<ProductCategory> returnList)
        {
            //get child of current item
            var childs = list.Where(a => a.ParentId == current.Id).OrderBy(r => r.Position).ToList();
            current.Childrens = new List<ProductCategory>();
            current.Childrens.AddRange(childs);
            foreach (var i in childs)
            {
                GetTreeview(list, i, ref returnList);
            }
        }

        public List<ProductCategory> BuildTree(bool? isActive, int language = 1)
        {
            var productCategories = GetActiveBaseContents(isActive, language);

            List<ProductCategory> list = productCategories.ToList();
            List<ProductCategory> returnList = new List<ProductCategory>();
            //find top levels items
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                GetTreeview(list, i, ref returnList);
            }
            return returnList;
        }
        public List<Node> CreateProductCategoryTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var productCategories = this.GetAll().OrderBy(r => r.Position).ToList();
            foreach (var p in productCategories)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId > 0 ? p.ParentId.ToStr() : "" });
            }

            return _lstTreeNodes;
        }
        public ProductCategory GetProductCategory(int categoryId)
        {
            //EImeceDbContext.Configuration.LazyLoadingEnabled = true;
            Expression<Func<ProductCategory, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<ProductCategory, object>> includeProperty2 = r => r.Products.Select(t => t.ProductFiles.Select(q => q.FileStorage));
            Expression<Func<ProductCategory, object>> includeProperty3 = r => r.Products.Select(t => t.ProductTags.Select(q => q.Tag));
            Expression<Func<ProductCategory, object>>[] includeProperties = { includeProperty1, includeProperty2, includeProperty3 };
            var item = GetSingleIncluding(categoryId, includeProperties);
            return item;
        }

    }
}
