using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ProductCommentService : BaseEntityService<ProductComment>, IProductCommentService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IProductCommentRepository ProductCommentRepository { get; set; }

        public ProductCommentService(IProductCommentRepository repository) : base(repository)
        {
            ProductCommentRepository = repository;
        }

        public List<ProductComment> GetAdminPageList(int? productId, string search, int lang, IList<int> ratings = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            return ProductCommentRepository.GetAdminPageList(productId, search, lang, ratings, startDate, endDate);
        }

        public async Task<List<ProductComment>> GetAdminPageListAsync(int? productId, string search, int lang, IList<int> ratings = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductCommentRepository.GetAdminPageListAsync(productId, search, lang, ratings, startDate, endDate, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Approved comments are embedded in the cached product-detail DTOs, so any comment
        /// mutation must drop the product:detail family or moderated comments stay visible
        /// (or invisible) until the detail TTL expires.
        /// </summary>
        protected override void InvalidateCachesAfterMutation()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.ProductDetailPrefix);
        }

        public override bool DeleteEntity(ProductComment entity)
        {
            var result = base.DeleteEntity(entity);
            InvalidateCachesAfterMutation();
            return result;
        }

        public override async Task<bool> DeleteEntityAsync(ProductComment entity)
        {
            var result = await base.DeleteEntityAsync(entity).ConfigureAwait(false);
            InvalidateCachesAfterMutation();
            return result;
        }

        public override void DeleteBaseEntity(List<string> values)
        {
            base.DeleteBaseEntity(values);
            InvalidateCachesAfterMutation();
        }

        public override async Task DeleteBaseEntityAsync(List<string> values)
        {
            await base.DeleteBaseEntityAsync(values).ConfigureAwait(false);
            InvalidateCachesAfterMutation();
        }
    }
}
