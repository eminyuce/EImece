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
using EImece.Domain.Helpers;
using System.Data.Entity.Validation;

namespace EImece.Domain.Repositories
{
    public abstract class BaseEntityRepository<T> : BaseRepository<T> where T : BaseEntity
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
        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search)
        {
            var menus = GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                menus = menus.Where(whereLambda);
            }
            return menus.OrderBy(r => r.Position).ThenByDescending(r => r.Id).ToList();
        }
      
    }
}
