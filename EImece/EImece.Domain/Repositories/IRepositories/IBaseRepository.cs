using GenericRepository;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseRepository<T> : IEntityRepository<T, int> where T : class, IEntity<int>
    {
        int SaveOrEdit(T item);

        int DeleteItem(T item);

        EntitiesContext GetDbContext();

        bool DeleteByWhereCondition(Expression<Func<T, bool>> whereLambda);

        T[] ExecuteStoreQuery(string commandText, params object[] parameters);

        void DeleteBaseEntity(List<string> values);
    }
}