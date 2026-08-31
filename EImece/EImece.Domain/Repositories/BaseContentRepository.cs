using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public abstract class BaseContentRepository<T> : BaseEntityRepository<T> where T : BaseContent
    {
        protected BaseContentRepository(IEImeceContext dbContext, ILogger logger) : base(dbContext, logger)
        {
        }

        protected BaseContentRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public virtual T GetBaseContent(int id)
        {
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var item = GetSingleIncluding(id, includeProperties);
            return item;
        }

        public virtual async Task<T> GetBaseContentAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var item = await GetSingleIncludingAsync(id, cancellationToken, includeProperties).ConfigureAwait(false);
            return item;
        }

        public virtual List<T> GetActiveBaseContents(bool? isActive, int? language)
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.Lang == (language.HasValue ? language.Value : r2.Lang);
                var predicate = PredicateBuilder.Create<T>(match);
                if (isActive != null && isActive.HasValue)
                {
                    predicate = predicate.And(r => r.IsActive == isActive);
                }
                Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
                Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = this.FindAllIncludingReadOnly(predicate, keySelector, OrderByType.Ascending, null, null, includeProperties);

                var result = items.ToList();

                return result == null ? new List<T>() : result;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception);
                throw;
            }
        }

        public virtual async Task<List<T>> GetActiveBaseContentsAsync(bool? isActive, int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.Lang == (language.HasValue ? language.Value : r2.Lang);
                var predicate = PredicateBuilder.Create<T>(match);
                if (isActive != null && isActive.HasValue)
                {
                    predicate = predicate.And(r => r.IsActive == isActive);
                }
                Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
                Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = this.FindAllIncludingReadOnly(predicate, keySelector, OrderByType.Ascending, null, null, includeProperties);

                var result = await items.ToListAsync(cancellationToken).ConfigureAwait(false);

                return result == null ? new List<T>() : result;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "GetActiveBaseContentsAsync failed.");
                throw new InvalidOperationException("GetActiveBaseContentsAsync failed.", exception);
            }
        }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            Expression<Func<T, bool>> match = r2 => r2.Lang == language;
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var menus = GetAllIncludingReadOnly(includeProperties.ToArray());

            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                match = match.And(whereLambda);
            }

            var result = menus.Where(match).OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();
            return result;
        }

        public virtual async Task<List<T>> SearchEntitiesAsync(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            Expression<Func<T, bool>> match = r2 => r2.Lang == language;
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var menus = GetAllIncludingReadOnly(includeProperties.ToArray());

            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                match = match.And(whereLambda);
            }

            var result = await menus.Where(match).OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToListAsync().ConfigureAwait(false);
            return result;
        }
    }
}
