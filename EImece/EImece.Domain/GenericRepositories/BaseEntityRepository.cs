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

namespace EImece.Domain.GenericRepositories
{
    public class BaseEntityRepository
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





        #region GenericMethods


        public static void ChangeGridBaseContentOrderingOrState<T>(IBaseRepository<T, int> repository, List<OrderingItem> values, String checkbox = "") where T : class, IEntity<int>
        {
            try
            {
                foreach (OrderingItem item in values)
                {
                    var t = repository.GetSingle(item.Id);
                    var baseContent = t as BaseContent;
                    if (baseContent != null)
                    {
                        if (String.IsNullOrEmpty(checkbox))
                        {
                            baseContent.Position = item.Position;
                        }
                        else if (checkbox.Equals("imagestate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            baseContent.ImageState = item.IsActive;
                        }
                        else if (checkbox.Equals("state", StringComparison.InvariantCultureIgnoreCase))
                        {
                            baseContent.IsActive = item.IsActive;
                        }
                        
                    }
                    repository.Edit(t);
                }
                repository.Save();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "ChangeGridOrderingOrState<T> :" + exception.StackTrace, String.Join(",", values));
            }
        }


        public static void ChangeGridBaseEntityOrderingOrState<T>(IBaseRepository<T, int> repository, List<OrderingItem> values, String checkbox = "") where T : class, IEntity<int>
        {
            try
            {
                foreach (OrderingItem item in values)
                {
                    var t = repository.GetSingle(item.Id);
                    var baseContent = t as BaseEntity;
                    if (baseContent != null)
                    {
                        if (String.IsNullOrEmpty(checkbox))
                        {
                            baseContent.Position = item.Position;
                        }
                        else if (checkbox.Equals("state", StringComparison.InvariantCultureIgnoreCase))
                        {
                            baseContent.IsActive = item.IsActive;
                        }

                    }
                    repository.Edit(t);
                }
                repository.Save();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "ChangeGridOrderingOrState<T> :" + String.Join(",", values), checkbox);
            }
        }
        public static void DeleteBaseEntity<T>(IBaseRepository<T, int> repository, List<string> values) where T : class, IEntity<int>
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    var item = repository.GetSingle(id);
                    repository.Delete(item);
                }
                repository.Save();
            }
            catch (DbEntityValidationException ex)
            {
                var message = GetDbEntityValidationExceptionDetail(ex);
                Logger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
        public static List<T> GetActiveBaseEnities<T>(IBaseRepository<T, int> repository, int? take, bool? isActive) where T : BaseEntity
        {
            try
            {
                Expression<Func<T, bool>> match = r2 =>  r2.IsActive == (isActive.HasValue ? isActive.Value : r2.IsActive);
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = repository.FindAll(match, keySelector, OrderByType.Descending, take, null);

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }
        }
       

        #endregion
    }
}
