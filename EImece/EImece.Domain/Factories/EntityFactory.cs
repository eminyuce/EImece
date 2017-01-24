using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Factories
{
    public class EntityFactory : IEntityFactory
    {
        [Inject]
        public ISettingService SettingService { get; set; }

        public T GetBaseContentInstance<T>() where T : BaseContent, new()
        {
            T item = new T();
            item.IsActive = true;
            item.Lang = Settings.MainLanguage;
            item.ImageHeight = SettingService.GetSettingByKey("DefaultImageHeight").ToInt();
            item.ImageWidth = SettingService.GetSettingByKey("DefaultImageWidth").ToInt();
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
