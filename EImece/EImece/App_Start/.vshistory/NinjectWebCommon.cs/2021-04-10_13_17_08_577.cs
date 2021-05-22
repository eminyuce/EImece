[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(EImece.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(EImece.App_Start.NinjectWebCommon), "Stop")]

namespace EImece.App_Start
{
    using Domain.ApiRepositories;
    using Domain.Caching;
    using Domain.DbContext;
    using Domain.Factories;
    using Domain.Factories.IFactories;
    using Domain.Helpers;
    using Domain.Helpers.EmailHelper;
    using Domain.Repositories;
    using Domain.Repositories.IRepositories;
    using Domain.Services;
    using Domain.Services.IServices;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Ninject;
    using Ninject.Web.Common;
    using NLog;
    using Quartz;
    using Quartz.Impl;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web;

    public static class NinjectWebCommon
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
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
        public static IKernel CreateKernel()
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
            kernel.Bind<IEimeceCacheProvider>().To<LazyCacheProvider>().InSingletonScope();
            kernel.Bind<IEmailSender>().To<EmailSender>().InRequestScope();

            var m = kernel.Bind<IEImeceContext>().To<EImeceContext>();
            m.WithConstructorArgument("nameOrConnectionString", Domain.Constants.DbConnectionKey);
            m.InRequestScope();

            kernel.Bind<IEntityFactory>().To<EntityFactory>();

            BindServices(kernel);
            BindRepositories(kernel);

            //        BindByReflection(kernel, typeof(IBaseEntityService<>), "Service");
            //      BindByReflection(kernel, typeof(IBaseRepository<>), "Repository");

            kernel.Bind<FilesHelper>().ToSelf().InRequestScope();
            kernel.Bind<AdresService>().ToSelf().InRequestScope();
            kernel.Bind<IyzicoService>().ToSelf().InRequestScope();
            kernel.Bind<MigrationRepository>().ToSelf().InRequestScope();
            kernel.Bind<SiteMapService>().ToSelf().InRequestScope();
            kernel.Bind<XmlEditorHelper>().ToSelf().InRequestScope();

            kernel.Bind<BitlyRepository>().ToSelf().InRequestScope();

            kernel.Bind<IdentityManager>().ToSelf().InRequestScope();
            kernel.Bind<ApplicationUserManager>().ToSelf().InRequestScope();
            kernel.Bind<ApplicationSignInManager>().ToSelf().InRequestScope();
            kernel.Bind<ApplicationDbContext>().ToSelf().InRequestScope();
            kernel.Bind<AppLogRepository>().ToSelf().InRequestScope();
            //kernel.Bind<StdSchedulerFactory>().ToSelf().InRequestScope();
            //kernel.Bind<QuartzService>().ToSelf().InRequestScope();

            // setup Quartz scheduler that uses our NinjectJobFactory

            kernel.Bind<Task<IScheduler>>().ToMethod(x =>
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                // get a scheduler
                var sched = factory.GetScheduler();
                return sched;
            });

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

            kernel.Bind<IHttpContextFactory>().To<HttpContextFactory>();

            kernel.Bind<RazorEngineHelper>().ToSelf().InRequestScope();
            kernel.Bind<UsersService>().ToSelf().InRequestScope();
        }

        private static void BindServices(IKernel kernel)
        {
            kernel.Bind<IFileStorageService>().To<FileStorageService>().InRequestScope();
            kernel.Bind<IListItemService>().To<ListItemService>().InRequestScope();
            kernel.Bind<IListService>().To<ListService>().InRequestScope();
            kernel.Bind<IMailTemplateService>().To<MailTemplateService>().InRequestScope();
            kernel.Bind<IMainPageImageService>().To<MainPageImageService>().InRequestScope();
            kernel.Bind<IMenuService>().To<MenuService>().InRequestScope();
            kernel.Bind<IProductCategoryService>().To<ProductCategoryService>().InRequestScope();
            kernel.Bind<IProductService>().To<ProductService>().InRequestScope();
            kernel.Bind<ISettingService>().To<SettingService>().InRequestScope();
            kernel.Bind<IStoryCategoryService>().To<StoryCategoryService>().InRequestScope();
            kernel.Bind<IStoryService>().To<StoryService>().InRequestScope();
            kernel.Bind<ISubscriberService>().To<SubscriberService>().InRequestScope();
            kernel.Bind<ITagCategoryService>().To<TagCategoryService>().InRequestScope();
            kernel.Bind<ITagService>().To<TagService>().InRequestScope();
            kernel.Bind<ITemplateService>().To<TemplateService>().InRequestScope();

            kernel.Bind<IAddressService>().To<AddressService>().InRequestScope();
            kernel.Bind<ICustomerService>().To<CustomerService>().InRequestScope();
            kernel.Bind<IShoppingCartService>().To<ShoppingCartService>().InRequestScope();

            kernel.Bind<IOrderService>().To<OrderService>().InRequestScope();
            kernel.Bind<IOrderProductService>().To<OrderProductService>().InRequestScope();

            kernel.Bind<IFaqService>().To<FaqService>().InRequestScope();
            kernel.Bind<IProductCommentService>().To<ProductCommentService>().InRequestScope();
            kernel.Bind<IBrandService>().To<BrandService>().InRequestScope();
        }

        private static void BindRepositories(IKernel kernel)
        {
            kernel.Bind<IFileStorageRepository>().To<FileStorageRepository>().InRequestScope();
            kernel.Bind<IFileStorageTagRepository>().To<FileStorageTagRepository>().InRequestScope();
            kernel.Bind<IListItemRepository>().To<ListItemRepository>().InRequestScope();
            kernel.Bind<IListRepository>().To<ListRepository>().InRequestScope();
            kernel.Bind<IMailTemplateRepository>().To<MailTemplateRepository>().InRequestScope();
            kernel.Bind<IMainPageImageRepository>().To<MainPageImageRepository>().InRequestScope();
            kernel.Bind<IMenuFileRepository>().To<MenuFileRepository>().InRequestScope();
            kernel.Bind<IMenuRepository>().To<MenuRepository>().InRequestScope();
            kernel.Bind<IProductCategoryRepository>().To<ProductCategoryRepository>().InRequestScope();
            kernel.Bind<IProductFileRepository>().To<ProductFileRepository>().InRequestScope();
            kernel.Bind<IProductRepository>().To<ProductRepository>().InRequestScope();
            kernel.Bind<IProductSpecificationRepository>().To<ProductSpecificationRepository>().InRequestScope();
            kernel.Bind<IProductTagRepository>().To<ProductTagRepository>().InRequestScope();
            kernel.Bind<ISettingRepository>().To<SettingRepository>().InRequestScope();
            kernel.Bind<IShortUrlRepository>().To<ShortUrlRepository>().InRequestScope();
            kernel.Bind<IStoryCategoryRepository>().To<StoryCategoryRepository>().InRequestScope();
            kernel.Bind<IStoryFileRepository>().To<StoryFileRepository>().InRequestScope();
            kernel.Bind<IStoryRepository>().To<StoryRepository>().InRequestScope();
            kernel.Bind<IStoryTagRepository>().To<StoryTagRepository>().InRequestScope();
            kernel.Bind<ISubscriberRepository>().To<SubscriberRepository>().InRequestScope();
            kernel.Bind<ITagCategoryRepository>().To<TagCategoryRepository>().InRequestScope();
            kernel.Bind<ITagRepository>().To<TagRepository>().InRequestScope();
            kernel.Bind<ITemplateRepository>().To<TemplateRepository>().InRequestScope();
            kernel.Bind<IAddressRepository>().To<AddressRepository>().InRequestScope();
            kernel.Bind<ICustomerRepository>().To<CustomerRepository>().InRequestScope();
            kernel.Bind<IShoppingCartRepository>().To<ShoppingCartRepository>().InRequestScope();
            kernel.Bind<IOrderRepository>().To<OrderRepository>().InRequestScope();
            kernel.Bind<IOrderProductRepository>().To<OrderProductRepository>().InRequestScope();
            kernel.Bind<IFaqRepository>().To<FaqRepository>().InRequestScope();
            kernel.Bind<IProductCommentRepository>().To<ProductCommentRepository>().InRequestScope();
            kernel.Bind<IBrandRepository>().To<BrandRepository>().InRequestScope();
        }

        private static void BindByReflection(IKernel kernel, Type typeOfInterface, string typeofText)
        {
            var baseServiceTypes = Assembly.GetAssembly(typeOfInterface)
                .GetTypes().Where(t => t.Name.Contains(typeofText)).ToList();

            foreach (var type in baseServiceTypes)
            {
                var interfaceType = type.GetInterface("I" + type.Name);
                if (interfaceType != null)
                {
                    kernel.Bind(interfaceType).To(type).InRequestScope();
                    Logger.Trace("kernel.Bind<" + interfaceType + ">().To<" + type + ">().InRequestScope();");
                }
            }
        }
    }
}