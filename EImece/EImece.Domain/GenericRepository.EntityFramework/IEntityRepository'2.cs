using EImece.Domain.GenericRepository.EntityFramework.Enums;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

// Entity Framework 6 to support async methods
namespace EImece.Domain.GenericRepository.EntityFramework
{
    /// <summary>
    /// Entity Framework interface implementation for IRepository.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TId">Type of entity Id</typeparam>
    public interface IEntityRepository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>
        where TId : IComparable
    {
        IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity GetSingleIncluding(TId id, params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity GetSingleIncluding(TId id, Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        PaginatedList<TEntity> Paginate<TKey>(
            int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> keySelector);

        PaginatedList<TEntity> Paginate<TKey>(
            int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        PaginatedList<TEntity> PaginateDescending<TKey>(
            int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> keySelector);

        PaginatedList<TEntity> PaginateDescending<TKey>(
            int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        void Add(TEntity entity);

        void AddGraph(TEntity entity);

        void Edit(TEntity entity);

        void Delete(TEntity entity);

        int Save();

        IQueryable<TEntity> FindAll<TKey>(Expression<Func<TEntity, bool>> match, Expression<Func<TEntity, TKey>> keySelector,
                                    OrderByType orderByType, int? take, int? skip);

        int Count();

        int Count(Expression<Func<TEntity, bool>> match);

        IQueryable<TEntity> FindAllIncluding<TKey>(Expression<Func<TEntity, bool>> match, Expression<Func<TEntity, TKey>> keySelector, OrderByType orderByType, int? take, int? skip, params Expression<Func<TEntity, object>>[] includeProperties);

        bool Contains(Expression<Func<TEntity, bool>> predicate);

        void Delete(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Executes the procedure.
        /// </summary>
        /// <param name="procedureCommand">The procedure command.</param>
        /// <param name="sqlParams">The SQL params.</param>
        void ExecuteProcedure(String procedureCommand, params SqlParameter[] sqlParams);
    }
}