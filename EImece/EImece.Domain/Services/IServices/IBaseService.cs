using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseService<T, TId> where T : class, IEntity<TId> where TId : IComparable
    {

        IQueryable<T> LoadEntites(Expression<Func<T, bool>> whereLambda);

        //IQueryable<T> LoadEntites(Func<T, bool> whereLambda, int pageIndex, int pageSize, out int totalCount);

        T SaveOrEditEntity(T entity);

        bool DeleteEntity(T entity);

        bool DeleteEntityByWhere(Expression<Func<T, bool>> whereLambda);

        T[] ExecuteStoreQuery<T>(string commandText, params object[] parameters);
    }
}
