using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseContentService<T> : IBaseEntityService<T> where T : BaseContent
    {
        List<T> GetActiveBaseContents(bool? isActive, int? language);

        Task<List<T>> GetActiveBaseContentsAsync(bool? isActive, int? language, CancellationToken cancellationToken = default(CancellationToken));

        List<T> GetActiveBaseContentsFromCache(bool? isActive, int? language);

        Task<List<T>> GetActiveBaseContentsFromCacheAsync(bool? isActive, int? language);

        new void DeleteBaseEntity(List<string> values);

        T GetBaseContent(int id);

        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language);

        Task<List<T>> SearchEntitiesAsync(Expression<Func<T, bool>> whereLambda, String search, int language);
    }
}