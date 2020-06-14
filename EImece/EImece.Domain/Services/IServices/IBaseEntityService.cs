using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseEntityService<T> : IBaseService<T> where T : BaseEntity
    {
        void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "");

        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int? language);

        List<T> GetActiveBaseEntities(bool? isActive, int? language);

        List<T> GetActiveBaseEntitiesFromCache(bool? isActive, int? language);
    }
}