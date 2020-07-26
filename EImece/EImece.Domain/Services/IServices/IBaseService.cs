using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseService<T> where T : class
    {
        bool IsCachingActive { get; set; }

        List<T> LoadEntites(Expression<Func<T, bool>> whereLambda);

        //IQueryable<T> LoadEntites(Func<T, bool> whereLambda, int pageIndex, int pageSize, out int totalCount);

        T SaveOrEditEntity(T entity);

        T GetSingle(int id);

        List<T> GetAll();

        bool DeleteEntity(T entity);

        void DeleteBaseEntity(List<string> values);

        bool DeleteById(int id);
    }
}