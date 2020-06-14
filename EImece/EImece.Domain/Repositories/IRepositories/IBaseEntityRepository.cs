using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseEntityRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        List<T> GetActiveBaseEntities(bool? isActive, int? language);

        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int? language);
    }
}