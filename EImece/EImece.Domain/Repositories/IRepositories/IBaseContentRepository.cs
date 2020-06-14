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

        new List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language);
    }
}