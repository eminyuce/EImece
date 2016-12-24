using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseEntityService<T> : IBaseService<T, int> where T : BaseEntity
    {
        void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "");

    }
}
