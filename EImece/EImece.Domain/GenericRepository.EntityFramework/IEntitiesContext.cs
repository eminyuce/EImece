using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace EImece.Domain.GenericRepository.EntityFramework
{
    public interface IEntitiesContext : IDisposable
    {
        IDbSet<TEntity> Set<TEntity>() where TEntity : class;

        void SetAsAdded<TEntity>(TEntity entity) where TEntity : class;

        void SetAsModified<TEntity>(TEntity entity) where TEntity : class;

        void SetAsDeleted<TEntity>(TEntity entity) where TEntity : class;

        int SaveChanges();

        Task<int> SaveChangesAsync();

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}