using EImece.Domain.DbContext;
using EImece.Domain.GenericRepository;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    /// <summary>
    /// The only layer allowed to query EF during data export operations.
    /// </summary>
    public class DataExportRepository : IDataExportRepository
    {
        private readonly IEImeceContext _dbContext;

        public DataExportRepository(IEImeceContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<List<T>> GetPageAsync<T>(int skip, int take, CancellationToken cancellationToken) where T : class, IEntity<int>
        {
            return _dbContext.Set<T>()
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetEntityCountsAsync(CancellationToken cancellationToken)
        {
            var counts = new Dictionary<string, int>();
            counts["Settings"] = await _dbContext.Settings.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["MailTemplates"] = await _dbContext.MailTemplates.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Faqs"] = await _dbContext.Faqs.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Subscribers"] = await _dbContext.Subscribers.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["FileStorages"] = await _dbContext.FileStorages.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["ProductCategories"] = await _dbContext.ProductCategories.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Brands"] = await _dbContext.Brands.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Products"] = await _dbContext.Products.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["ProductSpecifications"] = await _dbContext.ProductSpecifications.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["ProductFiles"] = await _dbContext.ProductFiles.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["ProductTags"] = await _dbContext.ProductTags.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["ProductComments"] = await _dbContext.ProductComments.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Coupons"] = await _dbContext.Coupons.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["StoryCategories"] = await _dbContext.StoryCategories.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Stories"] = await _dbContext.Stories.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Menus"] = await _dbContext.Menus.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Customers"] = await _dbContext.Customers.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Addresses"] = await _dbContext.Addresses.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["Orders"] = await _dbContext.Orders.CountAsync(cancellationToken).ConfigureAwait(false);
            counts["OrderProducts"] = await _dbContext.OrderProducts.CountAsync(cancellationToken).ConfigureAwait(false);
            return counts;
        }
    }
}
