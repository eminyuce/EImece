using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using GenericRepository.EntityFramework.Enums;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class ProductCategoryRepository : BaseContentRepository<ProductCategory>, IProductCategoryRepository
    {
        [Inject]
        public IProductService ProductService { get; set; }

        public ProductCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            var pcList = GetAll();
            bool isActived = isActive != null && isActive.HasValue;
            if (isActived)
            {
                pcList = pcList.Where(r => r.IsActive == isActive);
            }
            pcList = pcList.Where(r => r.Lang == language);
            var productCategories = pcList.OrderBy(r => r.Position).Select(c => new { ProductCategory = c, ProductCount = c.Products.Where(r => isActived ? r.IsActive : true).Count() });

            List<ProductCategoryTreeModel> list = productCategories.Select(r => new ProductCategoryTreeModel() { ProductCategory = r.ProductCategory, ProductCount = r.ProductCount, ProductCountAdmin = r.ProductCount }).ToList();
            List<ProductCategoryTreeModel> returnList = new List<ProductCategoryTreeModel>();

            int level = 1;
            //find top levels items
            var topLevels = list.Where(a => a.ProductCategory.ParentId == 0).OrderBy(r => r.ProductCategory.Position).ToList();
            topLevels.ForEach(r => r.TreeLevel = level);
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                GetTreeview(list, i, level);
            }
            return returnList;
        }

        //Recursion method for recursively get all child nodes
        private void GetTreeview(List<ProductCategoryTreeModel> list, ProductCategoryTreeModel current, int level)
        {
            //get child of current item
            var childs = list.Where(a => a.ProductCategory.ParentId == current.ProductCategory.Id).OrderBy(r => r.ProductCategory.Position).ToList();
            current.Childrens = new List<ProductCategoryTreeModel>();
            level = level + 1;
            childs.ForEach(r => r.TreeLevel = level);

            current.Childrens.AddRange(childs);
            foreach (var i in childs)
            {
                i.ProductCategory.Parent = current.ProductCategory;
                i.Parent = current;
                GetTreeview(list, i, level);
                current.ProductCount += i.ProductCount;
            }
        }

        public ProductCategory GetProductCategory(int categoryId, bool isOnlyActive = true)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Products);
            includeProperties.Add(r => r.Products.Select(t => t.MainImage));
            includeProperties.Add(r => r.Products.Select(t => t.ProductFiles.Select(q => q.FileStorage)));
            includeProperties.Add(r => r.Products.Select(t => t.ProductTags.Select(q => q.Tag)));
            if (isOnlyActive)
            {
                Expression<Func<ProductCategory, bool>> match = r => r.IsActive;
                return GetSingleIncluding(categoryId, includeProperties.ToArray(), match);
            }
            else
            {
                return GetSingleIncluding(categoryId, includeProperties.ToArray());
            }
        }

        public List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language)
        {
            var productCategories = GetActiveBaseContents(isActive, language);
            var result = new List<ProductCategory>();

            foreach (var m in productCategories)
            {
                if (!productCategories.Any(r => r.ParentId == m.Id))
                {
                    result.Add(m);
                }
            }

            return result;
        }

        public List<ProductCategory> GetMainPageProductCategories(int language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r => r.MainPage && r.IsActive && r.Lang == language;
            var result = FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray());

            return result.ToList();
        }

        public List<ProductCategory> GetAdminProductCategories(string search, int language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Template);

            Expression<Func<ProductCategory, bool>> match = r =>
             r.Lang == language;
            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                Expression<Func<ProductCategory, bool>> match2 = r => r.Name.Contains(search);
                match = match.And(match2);
            }
            var result = FindAllIncluding(match,
                r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray());

            return result.ToList();
        }

        public List<ProductCategory> GetProductCategoriesByParentId(int parentId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r =>
          r.ParentId == parentId && r.IsActive;

            var items = FindAllIncluding(match,
                 r => r.Position, OrderByType.Ascending, null, null,
                includeProperties.ToArray());
            var result = items.ToList();
            return result;
        }
    }
}