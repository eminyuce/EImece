using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Factories
{
    public class EntityFactory : IEntityFactory
    {
        public T GetBaseContentInstance<T>() where T : BaseContent, new()
        {
            T item = new T();
            item.IsActive = true;
            item.Lang = Settings.MainLanguage;
            item.ImageHeight = 0;
            item.ImageWidth = 0;
            return item;
        }

        public T GetBaseEntityInstance<T>() where T : BaseEntity, new()
        {
            T item = new T();
            item.IsActive = true;
            return item;
        }
        

    }
}
