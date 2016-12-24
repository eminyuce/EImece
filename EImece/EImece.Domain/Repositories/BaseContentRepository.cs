using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
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
        public virtual List<T> GetActiveBaseContents(bool? isActive, int language)
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.Lang == language;
                var predicate = PredicateBuilder.Create<T>(match);
                if (isActive != null && isActive.HasValue)
                {
                    predicate = predicate.And(r => r.IsActive == isActive);
                }
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = this.FindAll(predicate, keySelector, OrderByType.Ascending, null, null);

                return items;
            }
            catch (Exception exception)
            {
                BaseContentLogger.Error(exception);
                return null;
            }
        }
    }
}
