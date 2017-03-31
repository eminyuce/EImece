[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(EImece.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(EImece.App_Start.NinjectWebCommon), "Stop")]

namespace EImece.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
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

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ICacheProvider>().To<MemoryCacheProvider>().InSingletonScope();
            kernel.Bind<IEmailSender>().To<EmailSender>().InRequestScope();

            var m = kernel.Bind<IEImeceContext>().To<EImeceContext>();
            m.WithConstructorArgument("nameOrConnectionString", Settings.DbConnectionKey);
            m.InRequestScope();

            kernel.Bind<IEntityFactory>().To<EntityFactory>();


            kernel.Bind<IFileStorageRepository>().To<FileStorageRepository>().InRequestScope();
            kernel.Bind<IMenuRepository>().To<MenuRepository>().InRequestScope();
            kernel.Bind<IProductFileRepository>().To<ProductFileRepository>().InRequestScope();
            kernel.Bind<IProductCategoryRepository>().To<ProductCategoryRepository>().InRequestScope();
            kernel.Bind<IProductSpecificationRepository>().To<ProductSpecificationRepository>().InRequestScope();
            kernel.Bind<IProductTagRepository>().To<ProductTagRepository>().InRequestScope();
            kernel.Bind<IProductRepository>().To<ProductRepository>().InRequestScope();
            kernel.Bind<IStoryCategoryRepository>().To<StoryCategoryRepository>().InRequestScope();
            kernel.Bind<IStoryFileRepository>().To<StoryFileRepository>().InRequestScope();
            kernel.Bind<IStoryRepository>().To<StoryRepository>().InRequestScope();
            kernel.Bind<IStoryTagRepository>().To<StoryTagRepository>().InRequestScope();
            kernel.Bind<ISubscriberRepository>().To<SubscriberRepository>().InRequestScope();
            kernel.Bind<ITagCategoryRepository>().To<TagCategoryRepository>().InRequestScope();
            kernel.Bind<ITagRepository>().To<TagRepository>().InRequestScope();
            kernel.Bind<ISettingRepository>().To<SettingRepository>().InRequestScope();
            kernel.Bind<ITemplateRepository>().To<TemplateRepository>().InRequestScope();
            kernel.Bind<IMenuFileRepository>().To<MenuFileRepository>().InRequestScope();
            kernel.Bind<IMainPageImageRepository>().To<MainPageImageRepository>().InRequestScope();
            kernel.Bind<IListItemRepository>().To<ListItemRepository>().InRequestScope();
            kernel.Bind<IListRepository>().To<ListRepository>().InRequestScope();
            kernel.Bind<IFileStorageTagRepository>().To<FileStorageTagRepository>().InRequestScope();

            kernel.Bind<IMailTemplateRepository>().To<MailTemplateRepository>().InRequestScope();

            kernel.Bind<IFileStorageService>().To<FileStorageService>().InRequestScope();
            kernel.Bind<ISettingService>().To<SettingService>().InRequestScope();
            kernel.Bind<IStoryCategoryService>().To<StoryCategoryService>().InRequestScope();
            kernel.Bind<IMenuService>().To<MenuService>().InRequestScope();
            kernel.Bind<IStoryService>().To<StoryService>().InRequestScope();
            kernel.Bind<IProductService>().To<ProductService>().InRequestScope();
            kernel.Bind<ISubscriberService>().To<SubscriberService>().InRequestScope();
            kernel.Bind<ITagService>().To<TagService>().InRequestScope();
            kernel.Bind<ITagCategoryService>().To<TagCategoryService>().InRequestScope();
            kernel.Bind<IProductCategoryService>().To<ProductCategoryService>().InRequestScope();
            kernel.Bind<ITemplateService>().To<TemplateService>().InRequestScope();
            kernel.Bind<IMainPageImageService>().To<MainPageImageService>().InRequestScope();

            kernel.Bind<IMailTemplateService>().To<MailTemplateService>().InRequestScope();

            kernel.Bind<IListItemService>().To<ListItemService>().InRequestScope();
            kernel.Bind<IListService>().To<ListService>().InRequestScope();


            kernel.Bind<FilesHelper>().ToSelf().InRequestScope();

            kernel.Bind<MigrationRepository>().ToSelf().InRequestScope();

            kernel.Bind<XmlEditorHelper>().ToSelf().InRequestScope();

            kernel.Bind<IdentityManager>().ToSelf().InRequestScope();
            kernel.Bind<ApplicationUserManager>().ToSelf().InRequestScope();
            kernel.Bind<ApplicationSignInManager>().ToSelf().InRequestScope();
            kernel.Bind<ApplicationDbContext>().ToSelf().InRequestScope();
            kernel.Bind<IdentityFactoryOptions<ApplicationUserManager>>()
              .ToMethod(x => new IdentityFactoryOptions<ApplicationUserManager>()
              {
                  DataProtectionProvider = Startup.DataProtectionProvider
              });

            kernel.Bind<IUserStore<ApplicationUser>>()
             .ToMethod(x => new UserStore<ApplicationUser>(
                x.Kernel.Get<ApplicationDbContext>()))
             .InRequestScope();

            kernel.Bind<IAuthenticationManager>()
              .ToMethod(x => HttpContext.Current.GetOwinContext().Authentication)
              .InRequestScope();

            kernel.Bind<IIdentityMessageService>().To(typeof(SmsService)).InRequestScope().Named("Sms");
            kernel.Bind<IIdentityMessageService>().To(typeof(EmailService)).InRequestScope().Named("Email");
        }        
    }
}
