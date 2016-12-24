using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using System.Linq.Expressions;
using GenericRepository.EntityFramework.Enums;
using NLog;

namespace EImece.Domain.Repositories
{
    public abstract class BaseEntityRepository<T> : BaseRepository<T, int> where T : BaseEntity
    {
        protected static readonly Logger BaseEntityLogger = LogManager.GetCurrentClassLogger();

        public BaseEntityRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
        public virtual List<T> GetActiveBaseEntities(bool? isActive) 
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.IsActive == (isActive.HasValue ? isActive.Value : r2.IsActive);
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = this.FindAll(match, keySelector, OrderByType.Ascending, null, null);

                return items;
            }
            catch (Exception exception)
            {
                BaseEntityLogger.Error(exception);
                return null;
            }
        }
    }
}
