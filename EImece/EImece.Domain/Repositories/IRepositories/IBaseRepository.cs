using EImece.Domain.GenericRepository;
using EImece.Domain.GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseRepository<T> : IEntityRepository<T, int> where T : class, IEntity<int>
    {
        int SaveOrEdit(T item);

        Task<int> SaveOrEditAsync(T item);

        int DeleteItem(T item);

        Task<int> DeleteItemAsync(T item);

        EntitiesContext GetDbContext();

        bool DeleteByWhereCondition(Expression<Func<T, bool>> whereLambda);

        Task<bool> DeleteByWhereConditionAsync(Expression<Func<T, bool>> whereLambda);

        T[] ExecuteStoreQuery(string commandText, params object[] parameters);

        void DeleteBaseEntity(List<string> values);

        Task DeleteBaseEntityAsync(List<string> values);
    }
}