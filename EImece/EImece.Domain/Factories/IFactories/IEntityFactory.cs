using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Factories.IFactories
{
    public interface IEntityFactory
    {
        T GetBaseEntityInstance<T>() where T : BaseEntity, new();
        T GetBaseContentInstance<T>() where T : BaseContent, new();
    }
}
