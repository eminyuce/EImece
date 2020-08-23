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

#pragma warning disable CS0109 // The member 'IBaseContentService<T>.SearchEntities(Expression<Func<T, bool>>, string, int)' does not hide an accessible member. The new keyword is not required.

        new List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language);

#pragma warning restore CS0109 // The member 'IBaseContentService<T>.SearchEntities(Expression<Func<T, bool>>, string, int)' does not hide an accessible member. The new keyword is not required.
    }
}