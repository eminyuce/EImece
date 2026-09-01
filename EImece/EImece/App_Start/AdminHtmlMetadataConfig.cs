using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Models.Admin;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EImece.App_Start
{
    /// <summary>
    /// Associates MVC [AllowHtml] metadata with Domain entity/view-model properties
    /// without referencing System.Web in EImece.Domain.
    /// </summary>
    public static class AdminHtmlMetadataConfig
    {
        public static void Register()
        {
            RegisterMetadata<Product, ProductHtmlMetadata>();
            RegisterMetadata<ProductCategory, ProductCategoryHtmlMetadata>();
            RegisterMetadata<Brand, BaseContentDescriptionMetadata>();
            RegisterMetadata<Menu, BaseContentDescriptionMetadata>();
            RegisterMetadata<Story, StoryHtmlMetadata>();
            RegisterMetadata<StoryCategory, BaseContentDescriptionMetadata>();
            RegisterMetadata<MainPageImage, BaseContentDescriptionMetadata>();
            RegisterMetadata<Faq, FaqHtmlMetadata>();
            RegisterMetadata<MailTemplate, MailTemplateHtmlMetadata>();
            RegisterMetadata<Setting, SettingHtmlMetadata>();
            RegisterMetadata<Customer, CustomerHtmlMetadata>();
            RegisterMetadata<SettingModel, SettingModelHtmlMetadata>();
            RegisterMetadata<SystemSettingModel, SystemSettingModelHtmlMetadata>();
        }

        private static void RegisterMetadata<TModel, TMetadata>()
        {
            TypeDescriptor.AddProviderTransparent(
                new AssociatedMetadataTypeTypeDescriptionProvider(typeof(TModel), typeof(TMetadata)),
                typeof(TModel));
        }
    }
}
