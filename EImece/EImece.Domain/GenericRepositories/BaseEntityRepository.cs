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

        public static int SaveOrEdit<T>(IBaseRepository<T> repository,T item) where T : class, IEntity<int>
        {
            if (item.Id == 0)
            {
                repository.Add(item);
            }
            else
            {
                repository.Edit(item);
            }

            return repository.Save();
        }

        public static int DeleteItem<T>(IBaseRepository<T> repository, T item) where T : class, IEntity<int>

        {
            repository.Delete(item);
            return repository.Save();
        }
        public static List<T> GetActiveBaseEntitiesSearchList<T>(IBaseRepository<T> repository,  string search) where T : BaseEntity
        {
            try
            {
                Expression<Func<T, bool>> match = r2 =>  r2.IsActive;
                var predicate = PredicateBuilder.Create<T>(match);
                if (!String.IsNullOrEmpty(search.ToStr()))
                {
                    predicate = predicate.And(r => r.Name.ToLower().Contains(search.ToLower().Trim()));
                }
                var items = repository.FindBy(predicate).OrderBy(r => r.Position).ThenByDescending(r => r.Name);

                return items.ToList();

            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }

        }


    

        public static void ChangeGridBaseEntityOrderingOrState<T>(IBaseRepository<T> repository, List<OrderingItem> values, String checkbox = "") where T : class, IEntity<int>
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
                        else if (checkbox.Equals("mainpage", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if(baseContent is Product)
                            {
                                var product = baseContent as Product;
                                product.MainPage = item.IsActive;
                            }
                        }
                        else if (checkbox.Equals("imagestate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (baseContent is BaseContent)
                            {
                                var product = baseContent as BaseContent;
                                product.ImageState = item.IsActive;
                            }
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
        public static void DeleteBaseEntity<T>(IBaseRepository<T> repository, List<string> values) where T : class, IEntity<int>

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
    
        public static List<T> GetActiveBaseEntities<T>(IBaseRepository<T> repository, bool? isActive) where T : BaseEntity
        {
            try
            {
                Expression<Func<T, bool>> match = r2 => r2.IsActive == (isActive.HasValue ? isActive.Value : r2.IsActive);
                Expression<Func<T, int>> keySelector = t => t.Position;
                var items = repository.FindAll(match, keySelector, OrderByType.Ascending, null, null);

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
