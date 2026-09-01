using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;

using System;

namespace EImece.Domain.Factories
{
    public class EntityFactory : IEntityFactory
    {
        private readonly ISettingService SettingService;

        public EntityFactory(ISettingService settingService)
        {
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        public T GetBaseContentInstance<T>() where T : BaseContent, new()
        {
            T item = new T();
            item.IsActive = true;
            item.ImageState = true;
            item.Lang = AppConfig.MainLanguage;
            item.ImageHeight = SettingService.GetSettingByKey(Constants.DefaultImageHeight).ToInt();
            item.ImageWidth = SettingService.GetSettingByKey(Constants.DefaultImageWidth).ToInt();
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