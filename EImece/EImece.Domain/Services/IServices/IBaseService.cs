using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        Task<bool> DeleteEntityAsync(T entity);

        void DeleteBaseEntity(List<string> values);

        Task DeleteBaseEntityAsync(List<string> values);

        bool DeleteById(int id);

        Task<bool> DeleteByIdAsync(int id);

        Task<T> SaveOrEditEntityAsync(T entity);

        Task<T> GetSingleAsync(int id);

        Task<List<T>> GetAllAsync();
    }
}