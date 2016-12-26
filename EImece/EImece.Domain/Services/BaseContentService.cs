using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework.Enums;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
   
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
        protected static readonly Logger BaseContentServiceLogger = LogManager.GetCurrentClassLogger();

        public IBaseContentRepository<T> BaseContentRepository { get; set; }
        protected BaseContentService(IBaseContentRepository<T> baseContentRepository) :base(baseContentRepository) 
        {
            this.BaseContentRepository = baseContentRepository;
        }
        public virtual List<T> GetActiveBaseContents(bool ?isActive, int language)
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
                var items = BaseContentRepository.FindAll(predicate, keySelector, OrderByType.Ascending, null, null);

                return items;
            }
            catch (Exception exception)
            {
                BaseContentServiceLogger.Error(exception);
                return null;
            }
        }
    }
}
