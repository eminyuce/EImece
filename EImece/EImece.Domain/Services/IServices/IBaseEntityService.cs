using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseEntityService<T> : IBaseService<T> where T : BaseEntity
    {
        void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "");
        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search);

    }
}
