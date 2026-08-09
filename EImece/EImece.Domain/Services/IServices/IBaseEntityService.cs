using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseEntityService<T> : IBaseService<T> where T : BaseEntity
    {
        void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "");

        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int? language);

        List<T> GetActiveBaseEntities(bool? isActive, int? language);

        List<T> GetActiveBaseEntitiesFromCache(bool? isActive, int? language);

        Task<List<T>> GetActiveBaseEntitiesAsync(bool? isActive, int? language, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<T>> GetActiveBaseEntitiesFromCacheAsync(bool? isActive, int? language);

        Task ChangeGridBaseEntityOrderingOrStateAsync(List<OrderingItem> values, String checkbox = "");

        Task<List<T>> SearchEntitiesAsync(Expression<Func<T, bool>> whereLambda, String search, int? language);
    }
}