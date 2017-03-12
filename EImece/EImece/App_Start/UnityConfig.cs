using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc5;
using Domain.DbContext;
using Domain.Repositories.IRepositories;
using Domain.Repositories;
using Domain;
using Domain.Services;
using Domain.Services.IServices;
using Domain.Helpers;
using Domain.Caching;
using Domain.Helpers.EmailHelper;
using Domain.Factories;
using Domain.Factories.IFactories;
using Microsoft.AspNet.Identity;
using Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

namespace EImece
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. 



             container.RegisterType<ICacheProvider, MemoryCacheProvider>();
             container.RegisterType<IEmailSender,EmailSender>();

            var m =  container.RegisterType<IEImeceContext,EImeceContext>();
            m.WithConstructorArgument("nameOrConnectionString", Settings.DbConnectionKey);
           

             container.RegisterType<IEntityFactory,EntityFactory>();


             container.RegisterType<IFileStorageRepository,FileStorageRepository>();
             container.RegisterType<IMenuRepository,MenuRepository>();
             container.RegisterType<IProductFileRepository,ProductFileRepository>();
             container.RegisterType<IProductCategoryRepository,ProductCategoryRepository>();
             container.RegisterType<IProductSpecificationRepository,ProductSpecificationRepository>();
             container.RegisterType<IProductTagRepository,ProductTagRepository>();
             container.RegisterType<IProductRepository,ProductRepository>();
             container.RegisterType<IStoryCategoryRepository,StoryCategoryRepository>();
             container.RegisterType<IStoryFileRepository,StoryFileRepository>();
             container.RegisterType<IStoryRepository,StoryRepository>();
             container.RegisterType<IStoryTagRepository,StoryTagRepository>();
             container.RegisterType<ISubscriberRepository,SubscriberRepository>();
             container.RegisterType<ITagCategoryRepository,TagCategoryRepository>();
             container.RegisterType<ITagRepository,TagRepository>();
             container.RegisterType<ISettingRepository,SettingRepository>();
             container.RegisterType<ITemplateRepository,TemplateRepository>();
             container.RegisterType<IMenuFileRepository,MenuFileRepository>();
             container.RegisterType<IMainPageImageRepository,MainPageImageRepository>();
             container.RegisterType<IListItemRepository,ListItemRepository>();
             container.RegisterType<IListRepository,ListRepository>();
             container.RegisterType<IFileStorageTagRepository,FileStorageTagRepository>();

             container.RegisterType<IFileStorageService,FileStorageService>();
             container.RegisterType<ISettingService,SettingService>();
             container.RegisterType<IStoryCategoryService,StoryCategoryService>();
             container.RegisterType<IMenuService,MenuService>();
             container.RegisterType<IStoryService,StoryService>();
             container.RegisterType<IProductService,ProductService>();
             container.RegisterType<ISubscriberService,SubscriberService>();
             container.RegisterType<ITagService,TagService>();
             container.RegisterType<ITagCategoryService,TagCategoryService>();
             container.RegisterType<IProductCategoryService,ProductCategoryService>();
             container.RegisterType<ITemplateService,TemplateService>();
             container.RegisterType<IMainPageImageService,MainPageImageService>();

             container.RegisterType<IListItemService,ListItemService>();
             container.RegisterType<IListService,ListService>();


             container.RegisterType<FilesHelper>().ToSelf();

             container.RegisterType<MigrationRepository>().ToSelf();

             container.RegisterType<XmlEditorHelper>().ToSelf();

             container.RegisterType<IdentityManager>().ToSelf();
             container.RegisterType<ApplicationUserManager>().ToSelf();
             container.RegisterType<ApplicationSignInManager>().ToSelf();
             container.RegisterType<ApplicationDbContext>().ToSelf();
             container.RegisterType<IdentityFactoryOptions<ApplicationUserManager>>()
              .ToMethod(x => new IdentityFactoryOptions<ApplicationUserManager>()
              {
                  DataProtectionProvider = Startup.DataProtectionProvider
              });

             container.RegisterType<IUserStore<ApplicationUser>>()
             .ToMethod(x => new UserStore<ApplicationUser>(
                x.Kernel.Get<ApplicationDbContext>()))
             ;

             container.RegisterType<IAuthenticationManager>()
              .ToMethod(x => HttpContext.Current.GetOwinContext().Authentication)
              ;

             container.RegisterType<IIdentityMessageService, typeof(SmsService)).Named("Sms");
             container.RegisterType<IIdentityMessageService, typeof(EmailService)).Named("Email");


            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}