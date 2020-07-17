using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using Ninject;

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
            item.ImageState = true;
            item.Lang = AppConfig.MainLanguage;
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