using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
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
        public ProductCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        private static Expression<Func<ProductCategory, StorefrontCategoryDto>> NavigationCategoryProjection
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
                    Position = c.Position,
                    Lang = c.Lang,
                    IsActive = c.IsActive,
                    MainPage = c.MainPage
                };
            }
        }

        private List<StorefrontCategoryDto> GetProjectedNavigationCategories(bool? isActive, int language)
        {
            var query = EImeceDbContext.ProductCategories.AsNoTracking().Where(c => c.Lang == language);
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }
            return query
                .OrderBy(c => c.Position)
                .Select(NavigationCategoryProjection)
                .ToList();
        }

        private static List<ProductCategoryTreeModel> AssembleTreeModels(List<StorefrontCategoryDto> categories)
        {
            return ProductCategoryTreeAssembler.Assemble(categories);
        }

        private static void GetProjectedTreeview(List<ProductCategoryTreeModel> list, ProductCategoryTreeModel current, int level)
        {
            ProductCategoryTreeAssembler.AttachChildren(list, current, level);
        }

        public List<ProductCategoryTreeModel> BuildNavigation(bool? isActive, int language = 1)
        {
            var pcList = GetProjectedNavigationCategories(isActive, language);
            return AssembleTreeModels(pcList);
        }

        public async Task<List<ProductCategoryTreeModel>> BuildNavigationAsync(bool? isActive, int language = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.ProductCategories.AsNoTracking().Where(c => c.Lang == language);
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }
            var pcList = await query
                .OrderBy(c => c.Position)
                .Select(NavigationCategoryProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return AssembleTreeModels(pcList);
        }

        [Timed("repo.product_category.build_tree")]

        public virtual List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1)
        {
            var pcList = GetProjectedNavigationCategories(isActive, language);
            bool isActived = isActive != null && isActive.HasValue;
            var categoryIds = pcList.Select(c => c.Id).ToList();
            var productCounts = EImeceDbContext.Products.AsNoTracking()
                .Where(p => categoryIds.Contains(p.ProductCategoryId) && p.Lang == language && (!isActived || p.IsActive))
                .GroupBy(p => p.ProductCategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.CategoryId, x => x.Count);

            foreach (var node in pcList)
            {
                node.ProductCount = productCounts.ContainsKey(node.Id) ? productCounts[node.Id] : 0;
            }
            return AssembleTreeModels(pcList);
        }

        [Timed("repo.product_category.build_tree_async")]

        public virtual async Task<List<ProductCategoryTreeModel>> BuildTreeAsync(bool? isActive, int language = 1)
        {
            var query = EImeceDbContext.ProductCategories.AsNoTracking().Where(c => c.Lang == language);
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }
            var pcList = await query
                .OrderBy(c => c.Position)
                .Select(NavigationCategoryProjection)
                .ToListAsync(CancellationToken.None)
                .ConfigureAwait(false);
            bool isActived = isActive != null && isActive.HasValue;
            var categoryIds = pcList.Select(c => c.Id).ToList();
            var productCountRows = await EImeceDbContext.Products.AsNoTracking()
                .Where(p => categoryIds.Contains(p.ProductCategoryId) && p.Lang == language && (!isActived || p.IsActive))
                .GroupBy(p => p.ProductCategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync(CancellationToken.None).ConfigureAwait(false);
            var productCounts = productCountRows.ToDictionary(x => x.CategoryId, x => x.Count);

            foreach (var node in pcList)
            {
                node.ProductCount = productCounts.ContainsKey(node.Id) ? productCounts[node.Id] : 0;
            }
            return AssembleTreeModels(pcList);
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
            Expression<Func<ProductCategory, bool>> match = r =>
             r.Lang == language;
            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                Expression<Func<ProductCategory, bool>> match2 = r => r.Name.Contains(search);
                match = match.And(match2);
            }
            var result = GetAllIncluding(includeProperties.ToArray()).Where(match)
                .OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id);

            return result.ToList();
        }

        public async Task<List<ProductCategory>> GetAdminProductCategoriesAsync(string search, int language, CancellationToken cancellationToken = default(CancellationToken))
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
            var result = GetAllIncluding(includeProperties.ToArray()).Where(match)
                .OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id);

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

        [Timed("repo.product_category.get_storefront_by_id")]

        public virtual async Task<StorefrontCategoryDto> GetStorefrontCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId && c.IsActive)
                .Select(CategoryProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.product_category.get_storefront_by_id_sync")]

        public virtual StorefrontCategoryDto GetStorefrontCategoryById(int categoryId)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId && c.IsActive)
                .Select(CategoryProjection)
                .FirstOrDefault();
        }

        public ProductCategoryDto GetProductCategoryDto(int categoryId)
        {
            var dto = EImeceDbContext.ProductCategories.AsNoTracking()
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
            FillProductCategoryDto(dto);
            return dto;
        }

        public async Task<ProductCategoryDto> GetProductCategoryDtoAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dto = await EImeceDbContext.ProductCategories.AsNoTracking()
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
            await FillProductCategoryDtoAsync(dto, cancellationToken).ConfigureAwait(false);
            return dto;
        }

        private void FillProductCategoryDto(ProductCategoryDto dto)
        {
            if (dto == null) return;
            ApplyCategoryComputedUrls(dto);

            dto.Children = EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.ParentId == dto.Id && c.IsActive)
                .OrderBy(c => c.Position)
                .Select(c => new ProductCategoryDto { Id = c.Id, Name = c.Name, Position = c.Position })
                .ToList();
        }

        private async Task FillProductCategoryDtoAsync(ProductCategoryDto dto, CancellationToken cancellationToken)
        {
            if (dto == null) return;
            ApplyCategoryComputedUrls(dto);

            dto.Children = await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.ParentId == dto.Id && c.IsActive)
                .OrderBy(c => c.Position)
                .Select(c => new ProductCategoryDto { Id = c.Id, Name = c.Name, Position = c.Position })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        private static void ApplyCategoryComputedUrls(ProductCategoryDto dto)
        {
            dto.SeoUrl = string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(dto.Name), GeneralHelper.ModifyId(dto.Id));
            var dummy = new ProductCategory { Id = dto.Id, Name = dto.Name };
            dto.DetailPageUrl = dummy.GetDetailPageUrl("Category", "ProductCategories");
            if (dto.MainImageId.HasValue && dto.MainImageId.Value > 0)
            {
                dto.MainImageUrl = dummy.GetCroppedImageUrl(dto.MainImageId.Value, 800, 0);
                dto.MainImageThumbnailUrl = dummy.GetCroppedImageUrl(dto.MainImageId.Value, 200, 200);
            }
        }

        [Timed("repo.product_category.get_main_page", "Time taken to get storefront main page categories from DB")]
        public virtual async Task<List<StorefrontCategoryDto>> GetStorefrontMainPageCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.MainPage && c.IsActive && c.Lang == language && c.Products.Any(p => p.IsActive))
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.product_category.get_main_page_sync")]
        public virtual List<StorefrontCategoryDto> GetStorefrontMainPageCategories(int language)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.MainPage && c.IsActive && c.Lang == language && c.Products.Any(p => p.IsActive))
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToList();
        }

        [Timed("repo.product_category.get_children", "Time taken to get storefront children categories from DB")]
        public virtual async Task<List<StorefrontCategoryDto>> GetStorefrontChildrenCategoriesAsync(int parentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.ParentId == parentId && c.IsActive)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.product_category.get_children_sync")]
        public virtual List<StorefrontCategoryDto> GetStorefrontChildrenCategories(int parentId)
        {
            return EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.ParentId == parentId && c.IsActive)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToList();
        }

        [Timed("repo.product_category.build_nav_tree")]

        public virtual async Task<List<StorefrontCategoryDto>> BuildStorefrontNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var allCategories = await EImeceDbContext.ProductCategories.AsNoTracking()
                .Where(c => c.IsActive && c.Lang == language)
                .OrderBy(c => c.Position)
                .Select(CategoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return AssembleCategoryTree(allCategories);
        }

        [Timed("repo.product_category.build_nav_tree_sync")]

        public virtual List<StorefrontCategoryDto> BuildStorefrontNavigationTree(int language)
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

        public async Task<List<ProductCategory>> GetProductCategoriesForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductCategories.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<ProductCategory> GetProductCategoriesForImageExport()
        {
            return EImeceDbContext.ProductCategories.AsNoTracking().ToList();
        }
    }
}