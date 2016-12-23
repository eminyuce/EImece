using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories;

namespace EImece.Domain.GenericRepositories
{
    public class BaseContentRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected static String GetDbEntityValidationExceptionDetail(DbEntityValidationException ex)
        {

            var errorMessages = (from eve in ex.EntityValidationErrors
                                 let entity = eve.Entry.Entity.GetType().Name
                                 from ev in eve.ValidationErrors
                                 select new
                                 {
                                     Entity = entity,
                                     PropertyName = ev.PropertyName,
                                     ErrorMessage = ev.ErrorMessage
                                 });

            var fullErrorMessage = string.Join("; ", errorMessages.Select(e => string.Format("[Entity: {0}, Property: {1}] {2}", e.Entity, e.PropertyName, e.ErrorMessage)));

            var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);


            return exceptionMessage;
        }
        public static List<T> GetActiveBaseContents<T>(IBaseRepository<T, int> repository, bool? isActive, int language) where T : BaseContent
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.Lang == language;
                var predicate = PredicateBuilder.Create<T>(match);
                if(isActive!=null && isActive.HasValue)
                {
                    predicate = predicate.And(r => r.IsActive == isActive);
                }
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = repository.FindAll(predicate, keySelector, OrderByType.Ascending, null, null);

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }
        }
    }
}
