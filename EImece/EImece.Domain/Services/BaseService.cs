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

namespace EImece.Domain.Services
{
    public abstract class BaseService<T, TId> where T : class, IEntity<TId> where TId : IComparable
    {
        public IBaseRepository<T, TId> baseRepository { get; set; }

        public abstract void SetCurrentRepository();


        public virtual IQueryable<T> LoadEntites(Expression<Func<T, bool>> whereLambda)
        {
            return baseRepository.FindBy(whereLambda);
        }


        //public virtual IQueryable<T> LoadEntites(Expression<Func<T, bool>> whereLambda, int pageIndex, int pageSize, out int totalCount)
        //{
        //    var item =  this.baseRepository.Paginate(pageIndex, pageSize,r=>r.Id, whereLambda);
        //    totalCount = item.TotalCount;
        //    return item;
        //}


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

    }
}
