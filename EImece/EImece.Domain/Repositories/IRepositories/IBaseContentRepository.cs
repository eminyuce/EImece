using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseContentRepository<T> : IBaseEntityRepository<T> where T : BaseContent
    {
        List<T> GetActiveBaseContents(bool? isActive, int? language);

        T GetBaseContent(int id);

#pragma warning disable CS0109 // The member 'IBaseContentRepository<T>.SearchEntities(Expression<Func<T, bool>>, string, int)' does not hide an accessible member. The new keyword is not required.

        new List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language);

#pragma warning restore CS0109 // The member 'IBaseContentRepository<T>.SearchEntities(Expression<Func<T, bool>>, string, int)' does not hide an accessible member. The new keyword is not required.
    }
}