using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using System.Linq.Expressions;
using GenericRepository.EntityFramework;
using System.Data.Entity.Infrastructure;
using Ninject;
using EImece.Domain.Helpers;
using System.Data.Entity.Validation;
using NLog;

namespace EImece.Domain.Services
{
    public abstract class BaseService<T> where T : class, IEntity<int>
    {
        protected static readonly Logger BaseServiceLogger = LogManager.GetCurrentClassLogger();


        private IBaseRepository<T> baseRepository { get; set; }

        protected BaseService(IBaseRepository<T> baseRepository)
        {
            this.baseRepository = baseRepository;
        }

        public virtual List<T> LoadEntites(Expression<Func<T, bool>> whereLambda)
        {
            return baseRepository.FindBy(whereLambda).ToList();
        }

        public virtual List<T> GetAll()
        {
            return baseRepository.GetAll().ToList();
        }
        //public virtual IQueryable<T> LoadEntites(Expression<Func<T, bool>> whereLambda, int pageIndex, int pageSize, out int totalCount)
        //{
        //    var item =  this.baseRepository.Paginate(pageIndex, pageSize,r=>r.Id, whereLambda);
        //    totalCount = item.TotalCount;
        //    return item;
        //}

        public virtual T GetSingle(int id)
        {
            return baseRepository.GetSingle(id);
        }

        public virtual T SaveOrEditEntity(T entity)
        {
            var tmp = baseRepository.SaveOrEdit(entity);
            return entity;
        }


        public virtual bool DeleteEntity(T entity)
        {
            var result = this.baseRepository.DeleteItem(entity);
            return result == 1;
        }


        public virtual bool DeleteEntityByWhere(Expression<Func<T, bool>> whereLambda)
        {
            return this.baseRepository.DeleteEntityByWhere(whereLambda);
        }

        public T[] ExecuteStoreQuery<T>(string commandText, params object[] parameters)
        {
            EntitiesContext objectContext = baseRepository.GetDbContext();
            DbRawSqlQuery<T> result = objectContext.Database.SqlQuery<T>(commandText, parameters);
            return result.ToArray();
        }


        public virtual String GetDbEntityValidationExceptionDetail(DbEntityValidationException ex)
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


        public virtual void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    var item = baseRepository.GetSingle(id);
                    baseRepository.Delete(item);
                }
                baseRepository.Save();
            }
            catch (DbEntityValidationException ex)
            {
                var message = GetDbEntityValidationExceptionDetail(ex);
                BaseServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                BaseServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        [Inject]
        public IProductTagRepository ProductTagRepository { get; set; }
        [Inject]
        public IFileStorageTagRepository FileStorageTagRepository { get; set; }
        [Inject]
        public IStoryTagRepository StoryTagRepository { get; set; }


    }
}
