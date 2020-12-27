using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseService<T> where T : class
    {
        List<T> LoadEntites(Expression<Func<T, bool>> whereLambda);

        //IQueryable<T> LoadEntites(Func<T, bool> whereLambda, int pageIndex, int pageSize, out int totalCount);
        bool IsCachingActivated { get; set; }
         
        T SaveOrEditEntity(T entity);

        T GetSingle(int id);

        List<T> GetAll();

        bool DeleteEntity(T entity);

        void DeleteBaseEntity(List<string> values);

        bool DeleteById(int id);
    }
}