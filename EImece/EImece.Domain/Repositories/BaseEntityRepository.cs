using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public abstract class BaseEntityRepository<T> : BaseRepository<T> where T : BaseEntity
    {
        protected static readonly Logger BaseEntityLogger = LogManager.GetCurrentClassLogger();

        protected BaseEntityRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public virtual List<T> GetActiveBaseEntities(bool? isActive, int? language)
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.Lang == (language.HasValue ? language.Value : r2.Lang) && r2.IsActive == (isActive.HasValue ? isActive.Value : r2.IsActive);
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = this.FindAll(match, keySelector, OrderByType.Ascending, null, null);

                return items.ToList();
            }
            catch (Exception exception)
            {
                BaseEntityLogger.Error(exception);
                throw;
            }
        }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int? language)
        {
            Expression<Func<T, bool>> match = r2 => r2.Lang == (language.HasValue ? language.Value : r2.Lang);
            var menus = GetAll();
            menus = menus.Where(match);
            search = search.ToStr().ToLower().Trim();
            if (!String.IsNullOrEmpty(search.Trim()))
            {
                menus = menus.Where(whereLambda);
            }
            var result = menus.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();
            return result;
        }

        public virtual async Task<List<T>> GetActiveBaseEntitiesAsync(bool? isActive, int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.Lang == (language.HasValue ? language.Value : r2.Lang) && r2.IsActive == (isActive.HasValue ? isActive.Value : r2.IsActive);
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = this.FindAll(match, keySelector, OrderByType.Ascending, null, null);

                return await items.ToListAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                BaseEntityLogger.Error(exception, "GetActiveBaseEntitiesAsync failed.");
                throw new InvalidOperationException("GetActiveBaseEntitiesAsync failed.", exception);
            }
        }

        public virtual async Task<List<T>> SearchEntitiesAsync(Expression<Func<T, bool>> whereLambda, String search, int? language)
        {
            Expression<Func<T, bool>> match = r2 => r2.Lang == (language.HasValue ? language.Value : r2.Lang);
            var menus = GetAll();
            menus = menus.Where(match);
            search = search.ToStr().ToLower().Trim();
            if (!String.IsNullOrEmpty(search.Trim()))
            {
                menus = menus.Where(whereLambda);
            }
            var result = await menus.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToListAsync().ConfigureAwait(false);
            return result;
        }
    }
}