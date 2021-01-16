using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        public void DeleteProductTags(int productId)
        {
            var productTags = GetAll().Where(r => r.ProductId == productId).ToList();
            foreach (var product in productTags)
            {
                Delete(product);
            }
            Save();
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
   
        public PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Product);
            includeProperties.Add(r => r.Product.MainImage);
            includeProperties.Add(r => r.Product.ProductCategory);
            return this.Paginate(pageIndex, pageSize, r => r.Product.Position, r => r.TagId == tagId, includeProperties.ToArray());
        }
        public PaginatedList<ProductTag> GetProductsByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Product);
            includeProperties.Add(r => r.Product.MainImage);
            includeProperties.Add(r => r.Product.ProductCategory);
            Expression<Func<ProductTag, bool>> match = r2 => r2.Tag.IsActive && r2.Tag.Lang == lang &&  r2.TagId == tagId;

            if (sorting == SortingType.LowHighPrice)
            {
                Expression<Func<ProductTag, double>> keySelector = t => t.Product.Price;
                return this.Paginate(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
            }
            else if (sorting == SortingType.HighLowPrice)
            {
                Expression<Func<ProductTag, double>> keySelector = t => t.Product.Price;
                return this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
            }
            else if (sorting == SortingType.Newest)
            {
                Expression<Func<ProductTag, DateTime>> keySelector = t => t.Product.UpdatedDate;
                return this.Paginate(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());
            }
            else
            {
                Expression<Func<ProductTag, double>> keySelector = t => t.Product.Position;
                return this.Paginate(pageIndex, pageSize, keySelector, r => r.TagId == tagId, includeProperties.ToArray());
            }

        }
    }
}