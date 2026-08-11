using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
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
    public class ProductTagRepository : BaseRepository<ProductTag>, IProductTagRepository
    {
        public ProductTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<ProductTag> GetAllByProductId(int productId)
        {
            return this.GetAll().Where(r => r.ProductId == productId).ToList();
        }

        public async Task<List<ProductTag>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetAll().Where(r => r.ProductId == productId).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public void DeleteProductTags(int productId)
        {
            var productTags = GetAll().Where(r => r.ProductId == productId).ToList();
            foreach (var product in productTags)
            {
                Delete(product);
            }
            Save();
        }

        public async Task DeleteProductTagsAsync(int productId)
        {
            var productTags = await GetAll().Where(r => r.ProductId == productId).ToListAsync().ConfigureAwait(false);
            foreach (var product in productTags)
            {
                Delete(product);
            }
            await SaveAsync().ConfigureAwait(false);
        }

        public void SaveProductTags(int productId, int[] tags)
        {
            DeleteProductTags(productId);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    ProductTag item = new ProductTag();
                    item.ProductId = productId;
                    item.TagId = tag;
                    this.Add(item);
                }
                Save();
            }
        }

        public async Task SaveProductTagsAsync(int productId, int[] tags)
        {
            await DeleteProductTagsAsync(productId).ConfigureAwait(false);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    ProductTag item = new ProductTag();
                    item.ProductId = productId;
                    item.TagId = tag;
                    this.Add(item);
                }
                await SaveAsync().ConfigureAwait(false);
            }
        }

        public PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Product);
            includeProperties.Add(r => r.Product.MainImage);
            includeProperties.Add(r => r.Product.ProductCategory);
            return GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(r => r.TagId == tagId)
                .OrderByProductStorefrontDefault()
                .ToPaginatedList(pageIndex, pageSize);
        }

        public async Task<PaginatedList<ProductTag>> GetProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Product);
            includeProperties.Add(r => r.Product.MainImage);
            includeProperties.Add(r => r.Product.ProductCategory);
            return await GetAllIncludingReadOnly(includeProperties.ToArray())
                .Where(r => r.TagId == tagId)
                .OrderByProductStorefrontDefault()
                .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken)
                .ConfigureAwait(false);
        }

        public PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Product);
            includeProperties.Add(r => r.Product.MainImage);
            includeProperties.Add(r => r.Product.ProductCategory);
            Expression<Func<ProductTag, bool>> match = r2 => r2.Tag.IsActive && r2.Tag.Lang == lang && r2.TagId == tagId;
            var query = GetAllIncludingReadOnly(includeProperties.ToArray()).Where(match);

            if (sorting == SortingType.LowHighPrice)
            {
                return query.OrderBy(t => t.Product.Price).ThenBy(t => t.Product.Position).ThenByDescending(t => t.Product.MainPage).ThenByDescending(t => t.Product.IsCampaign).ThenByDescending(t => t.Product.UpdatedDate)
                    .ToPaginatedList(pageIndex, pageSize);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                return query.OrderByDescending(t => t.Product.Price).ThenBy(t => t.Product.Position).ThenByDescending(t => t.Product.MainPage).ThenByDescending(t => t.Product.IsCampaign).ThenByDescending(t => t.Product.UpdatedDate)
                    .ToPaginatedList(pageIndex, pageSize);
            }
            else if (sorting == SortingType.Newest)
            {
                return query.OrderByDescending(t => t.Product.UpdatedDate).ThenBy(t => t.Product.Position).ThenByDescending(t => t.Product.MainPage).ThenByDescending(t => t.Product.IsCampaign)
                    .ToPaginatedList(pageIndex, pageSize);
            }
            else
            {
                return query.OrderByProductStorefrontDefault().ToPaginatedList(pageIndex, pageSize);
            }
        }

        public async Task<PaginatedList<ProductTag>> GetProductsByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Product);
            includeProperties.Add(r => r.Product.MainImage);
            includeProperties.Add(r => r.Product.ProductCategory);
            Expression<Func<ProductTag, bool>> match = r2 => r2.Tag.IsActive && r2.Tag.Lang == lang && r2.TagId == tagId;
            var query = GetAllIncludingReadOnly(includeProperties.ToArray()).Where(match);

            if (sorting == SortingType.LowHighPrice)
            {
                return await query.OrderBy(t => t.Product.Price).ThenBy(t => t.Product.Position).ThenByDescending(t => t.Product.MainPage).ThenByDescending(t => t.Product.IsCampaign).ThenByDescending(t => t.Product.UpdatedDate)
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                return await query.OrderByDescending(t => t.Product.Price).ThenBy(t => t.Product.Position).ThenByDescending(t => t.Product.MainPage).ThenByDescending(t => t.Product.IsCampaign).ThenByDescending(t => t.Product.UpdatedDate)
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
            else if (sorting == SortingType.Newest)
            {
                return await query.OrderByDescending(t => t.Product.UpdatedDate).ThenBy(t => t.Product.Position).ThenByDescending(t => t.Product.MainPage).ThenByDescending(t => t.Product.IsCampaign)
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await query.OrderByProductStorefrontDefault()
                    .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
