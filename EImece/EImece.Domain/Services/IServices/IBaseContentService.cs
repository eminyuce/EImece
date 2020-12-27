using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseContentService<T> : IBaseEntityService<T> where T : BaseContent
    {
        List<T> GetActiveBaseContents(bool? isActive, int? language);

        List<T> GetActiveBaseContentsFromCache(bool? isActive, int? language);

        new void DeleteBaseEntity(List<string> values);

        T GetBaseContent(int id);

        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language);
    }
}