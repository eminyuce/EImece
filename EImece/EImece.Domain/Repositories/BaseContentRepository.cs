using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public abstract class BaseContentRepository<T> : BaseEntityRepository<T> where T : BaseContent
    {
        protected static readonly Logger BaseContentLogger = LogManager.GetCurrentClassLogger();

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
                var items = this.FindAllIncluding(predicate, keySelector, OrderByType.Ascending, null, null, includeProperties);

                var result = items.ToList();

                return result == null ? new List<T>() : result;
            }
            catch (Exception exception)
            {
                BaseContentLogger.Error(exception);
                throw;
            }
        }

        public new virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            Expression<Func<T, bool>> match = r2 => r2.Lang == language;
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var menus = GetAllIncluding(includeProperties.ToArray());

            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                match = match.And(whereLambda);
            }

            var result = menus.Where(match).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
            return result;
        }
    }
}