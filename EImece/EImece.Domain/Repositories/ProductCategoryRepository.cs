using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class ProductCategoryRepository : BaseContentRepository<ProductCategory>, IProductCategoryRepository
    {
        [Inject]
        public IProductService ProductService { get; set; }

        public ProductCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<ProductCategoryTreeModel> BuildNavigation(bool? isActive, int language = 1)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r => r.Lang == language;
            bool isActived = isActive != null && isActive.HasValue;
            if (isActived)
            {
                match = match.And(r => r.IsActive == isActive);
            }
            var pcList = FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray()).ToList();

            List<ProductCategoryTreeModel> list = pcList.Select(r => new ProductCategoryTreeModel()
            {
                ProductCategory = r
            }).ToList();
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

        public List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r => r.Lang == language;
            bool isActived = isActive != null && isActive.HasValue;
            if (isActived)
            {
                match = match.And(r => r.IsActive == isActive);
            }
            var pcList = FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray()).ToList();
            var categoryIds = pcList.Select(c => c.Id).ToList();
            var productCounts = EImeceDbContext.Products.AsNoTracking()
                .Where(p => categoryIds.Contains(p.ProductCategoryId) && p.Lang == language && (!isActived || p.IsActive))
                .GroupBy(p => p.ProductCategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.CategoryId, x => x.Count);
            var productCategories = pcList.OrderBy(r => r.Position).Select(c =>
            new
            {
                ProductCategory = c,
                ProductCount = productCounts.ContainsKey(c.Id) ? productCounts[c.Id] : 0
            }).ToList();

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

        public async Task<List<ProductCategoryTreeModel>> BuildTreeAsync(bool? isActive, int language = 1)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r => r.Lang == language;
            bool isActived = isActive != null && isActive.HasValue;
            if (isActived)
            {
                match = match.And(r => r.IsActive == isActive);
            }
            var pcList = await FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray()).ToListAsync(CancellationToken.None).ConfigureAwait(false);
            var categoryIds = pcList.Select(c => c.Id).ToList();
            var productCountRows = await EImeceDbContext.Products.AsNoTracking()
                .Where(p => categoryIds.Contains(p.ProductCategoryId) && p.Lang == language && (!isActived || p.IsActive))
                .GroupBy(p => p.ProductCategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync(CancellationToken.None).ConfigureAwait(false);
            var productCounts = productCountRows.ToDictionary(x => x.CategoryId, x => x.Count);
            var productCategories = pcList.OrderBy(r => r.Position).Select(c =>
            new
            {
                ProductCategory = c,
                ProductCount = productCounts.ContainsKey(c.Id) ? productCounts[c.Id] : 0
            }).ToList();

            List<ProductCategoryTreeModel> list = productCategories.Select(r => new ProductCategoryTreeModel() { ProductCategory = r.ProductCategory, ProductCount = r.ProductCount, ProductCountAdmin = r.ProductCount }).ToList();
            List<ProductCategoryTreeModel> returnList = new List<ProductCategoryTreeModel>();

            int level = 1;
            var topLevels = list.Where(a => a.ProductCategory.ParentId == 0).OrderBy(r => r.ProductCategory.Position).ToList();
            topLevels.ForEach(r => r.TreeLevel = level);
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                GetTreeview(list, i, level);
            }
            return returnList;
        }

        private List<ProductCategory> GetActiveProductCategoriesWithActiveProducts(int language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Products);
            Expression<Func<ProductCategory, bool>> match = r => r.MainPage && r.IsActive && r.Lang == language && r.Products.Any(t => t.IsActive);
            var result = FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray());

            return result.ToList();
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
            if (isOnlyActive)
            {
                var result = GetSingleIncluding(categoryId, includeProperties.ToArray());
                if (result != null && result.IsActive)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return GetSingleIncluding(categoryId, includeProperties.ToArray());
            }
        }

        public async Task<ProductCategory> GetProductCategoryAsync(int categoryId, bool isOnlyActive = true)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            if (isOnlyActive)
            {
                var result = await GetSingleIncludingAsync(categoryId, CancellationToken.None, includeProperties.ToArray()).ConfigureAwait(false);
                if (result != null && result.IsActive)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return await GetSingleIncludingAsync(categoryId, CancellationToken.None, includeProperties.ToArray()).ConfigureAwait(false);
            }
        }

        public List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language)
        {
            var productCategories = GetActiveBaseContents(isActive, language);
            return productCategories.Where(m => !productCategories.Any(r => r.ParentId == m.Id)).ToList();
        }

        public async Task<List<ProductCategory>> GetProductCategoryLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var productCategories = await GetActiveBaseContentsAsync(isActive, language, cancellationToken).ConfigureAwait(false);
            return productCategories.Where(m => !productCategories.Any(r => r.ParentId == m.Id)).ToList();
        }

        public List<ProductCategory> GetMainPageProductCategories(int language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r => r.MainPage && r.IsActive && r.Lang == language;
            var result = FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray());

            return result.ToList();
        }

        public async Task<List<ProductCategory>> GetMainPageProductCategoriesAsync(int language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r => r.MainPage && r.IsActive && r.Lang == language;
            var result = FindAllIncluding(match, r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray());

            return await result.ToListAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public List<ProductCategory> GetAdminProductCategories(string search, int language)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Template);
            includeProperties.Add(r => r.Products);
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

        public async Task<List<ProductCategory>> GetAdminProductCategoriesAsync(string search, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Template);
            includeProperties.Add(r => r.Products);
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

            return await result.ToListAsync(cancellationToken).ConfigureAwait(false);
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

        public async Task<List<ProductCategory>> GetProductCategoriesByParentIdAsync(int parentId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<ProductCategory, bool>> match = r =>
          r.ParentId == parentId && r.IsActive;

            var items = FindAllIncluding(match,
                 r => r.Position, OrderByType.Ascending, null, null,
                includeProperties.ToArray());
            return await items.ToListAsync(CancellationToken.None).ConfigureAwait(false);
        }

        #region Storefront Read Implementations (LINQ Projection, AsNoTracking, Main Entity Activation)

        private static Expression<Func<ProductCategory, StorefrontCategoryDto>> CategoryProjection
        {
            get
            {
                return c => new StorefrontCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    ShortDescription = c.ShortDescription,
                    Description = c.Description,
                    MainImageId = c.MainImageId,
                    DiscountPercentage = c.DiscountPercantage.HasValue ? (int?)c.DiscountPercantage.Value : null,
                    Position = c.Position,
                    Lang = c.Lang,
                    IsActive = c.IsActive,
                    MainPage = c.MainPage,
                    TemplateId = c.TemplateId,
                    MetaKeywords = c.MetaKeywords,
                    ProductCount = c.Products.Count(p => p.IsActive)
                };
            }
        }

        private static Expression<Func<ProductCategory, StorefrontCategoryDto>> CategoryCardProjection
        {
            get
            {
                return c => new StorefrontCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    ShortDescription = c.ShortDescription,
                    MainImageId = c.MainImageId,
                    Position = c.Position,
                    Lang = c.Lang,
                    IsActive = c.IsActive,
                    MainPage = c.MainPage,
                    ProductCount = c.Products.Count(p => p.IsActive)
                };
            }
        }

        public async Task<StorefrontCategoryDto> GetStorefrontCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId && c.IsActive)
                .Select(CategoryProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontCategoryDto GetStorefrontCategoryById(int categoryId)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId && c.IsActive)
                .Select(CategoryProjection)
                .FirstOrDefault();
        }

        public ProductCategoryDto GetProductCategoryDto(int categoryId)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId && c.IsActive)
                .Select(c => new ProductCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ShortDescription = c.ShortDescription,
                    ParentId = c.ParentId,
                    MainPage = c.MainPage,
                    Position = c.Position,
                    Lang = c.Lang,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    MetaKeywords = c.MetaKeywords,
                    MainImageId = c.MainImageId,
                    TemplateId = c.TemplateId,
                    DiscountPercentage = c.DiscountPercantage
                })
                .FirstOrDefault();
        }

        public async Task<ProductCategoryDto> GetProductCategoryDtoAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId && c.IsActive)
                .Select(c => new ProductCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ShortDescription = c.ShortDescription,
                    ParentId = c.ParentId,
                    MainPage = c.MainPage,
                    Position = c.Position,
                    Lang = c.Lang,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    MetaKeywords = c.MetaKeywords,
                    MainImageId = c.MainImageId,
                    TemplateId = c.TemplateId,
                    DiscountPercentage = c.DiscountPercantage
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<StorefrontCategoryDto>> GetStorefrontMainPageCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.MainPage && c.IsActive && c.Lang == language && c.Products.Any(p => p.IsActive))
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> GetStorefrontMainPageCategories(int language)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.MainPage && c.IsActive && c.Lang == language && c.Products.Any(p => p.IsActive))
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToList();
        }

        public async Task<List<StorefrontCategoryDto>> GetStorefrontChildrenCategoriesAsync(int parentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.ParentId == parentId && c.IsActive)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> GetStorefrontChildrenCategories(int parentId)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.ParentId == parentId && c.IsActive)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToList();
        }

        public async Task<List<StorefrontCategoryDto>> BuildStorefrontNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var allCategories = await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.IsActive && c.Lang == language)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return AssembleCategoryTree(allCategories);
        }

        public List<StorefrontCategoryDto> BuildStorefrontNavigationTree(int language)
        {
            var allCategories = EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.IsActive && c.Lang == language)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToList();

            return AssembleCategoryTree(allCategories);
        }

        private static List<StorefrontCategoryDto> AssembleCategoryTree(List<StorefrontCategoryDto> allCategories)
        {
            var lookup = allCategories.ToLookup(c => c.ParentId);
            var roots = lookup[0].OrderBy(c => c.Position).ToList();

            void AttachChildren(StorefrontCategoryDto parent, int level)
            {
                parent.TreeLevel = level;
                parent.Children = lookup[parent.Id].OrderBy(c => c.Position).ToList();
                foreach (var child in parent.Children)
                {
                    AttachChildren(child, level + 1);
                    parent.ProductCount += child.ProductCount;
                }
            }

            foreach (var root in roots)
            {
                AttachChildren(root, 1);
            }

            return roots;
        }

        #endregion
    }
}