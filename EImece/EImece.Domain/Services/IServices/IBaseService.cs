using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseService<T> where T : class
    {

        IQueryable<T> LoadEntites(Func<T, bool> whereLambda);

        //IQueryable<T> LoadEntites(Func<T, bool> whereLambda, int pageIndex, int pageSize, out int totalCount);

        T SaveOrEditEntity(T entity);

        bool DeleteEntity(T entity);

        bool DeleteEntityByWhere(Func<T, bool> whereLambda);

        T[] ExecuteStoreQuery<T>(string commandText, params object[] parameters);
    }
}
