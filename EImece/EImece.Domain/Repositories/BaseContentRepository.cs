using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{

    public abstract class BaseContentRepository<T> : BaseEntityRepository<T> where T : BaseContent
    {
        protected static readonly Logger BaseContentLogger = LogManager.GetCurrentClassLogger();

        public BaseContentRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
        public virtual T GetBaseContent(int id)
        {
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var item = GetSingleIncluding(id, includeProperties);
            return item;
        }
        public virtual List<T> GetActiveBaseContents(bool? isActive, int ? language)
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
                var items = this.FindAllIncluding(predicate,keySelector, OrderByType.Ascending, null, null, includeProperties);

                return items.ToList();
            }
            catch (Exception exception)
            {
                BaseContentLogger.Error(exception);
                return null;
            }
        }

        public new virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            Expression<Func<T, bool>> match = r2 => r2.Lang == language;
            Expression<Func<T, object>> includeProperty1 = r => r.MainImage;
            Expression<Func<T, object>>[] includeProperties = { includeProperty1 };
            var menus = GetAllIncluding(includeProperties.ToArray());
            if (!String.IsNullOrEmpty(search.Trim()))
            {
                menus = menus.Where(whereLambda);
            }
            var result = menus.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
            return result;
        }
    }



}
