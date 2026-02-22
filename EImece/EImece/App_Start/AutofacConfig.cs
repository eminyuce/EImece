using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using EImece.Domain.ApiRepositories;
using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.Factories;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Security;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.App_Start
{
    public static class AutofacConfig
    {
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterFilterProvider();
            builder.RegisterModelBinderProvider();

            RegisterServices(builder);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            return container;
        }

        private static MapperConfiguration CreateAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

#if DEBUG
            config.AssertConfigurationIsValid();
#endif

            return config;
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<LazyCacheProvider>().As<IEimeceCacheProvider>().SingleInstance();
            builder.RegisterType<EmailSender>().As<IEmailSender>().InstancePerRequest();

            builder.Register(c =>
            {
                var factory = new LoggerFactory();
                factory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
                return factory;
            }).As<ILoggerFactory>().SingleInstance();

            builder.Register(_ => CreateAutoMapper()).As<MapperConfiguration>().SingleInstance();
            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerRequest();

            builder.RegisterType<EImeceContext>()
                .As<IEImeceContext>()
                .WithParameter("nameOrConnectionString", Domain.Constants.DbConnectionKey)
                .InstancePerRequest();

            builder.RegisterType<EntityFactory>().As<IEntityFactory>().InstancePerRequest();

            BindServices(builder);
            BindRepositories(builder);

            builder.RegisterType<FilesHelper>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<AdresService>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<IyzicoService>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<ReportService>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<MigrationRepository>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<SiteMapService>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<XmlEditorHelper>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<BitlyRepository>().InstancePerRequest().PropertiesAutowired();

            builder.RegisterType<IdentityManager>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<ApplicationUserManager>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<ApplicationSignInManager>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<ApplicationDbContext>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<AppLogRepository>().InstancePerRequest().PropertiesAutowired();

            builder.Register(_ =>
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                return factory.GetScheduler();
            }).As<Task<IScheduler>>().InstancePerRequest();

            builder.Register(_ => new IdentityFactoryOptions<ApplicationUserManager>
            {
                DataProtectionProvider = Startup.DataProtectionProvider
            }).As<IdentityFactoryOptions<ApplicationUserManager>>().InstancePerRequest();

            builder.Register(c => new UserStore<ApplicationUser>(c.Resolve<ApplicationDbContext>()))
                .As<IUserStore<ApplicationUser>>()
                .InstancePerRequest();

            builder.Register(_ => HttpContext.Current.GetOwinContext().Authentication)
                .As<IAuthenticationManager>()
                .InstancePerRequest();

            builder.RegisterType<SmsService>().Named<IIdentityMessageService>("Sms").InstancePerRequest();
            builder.RegisterType<EmailService>().Named<IIdentityMessageService>("Email").InstancePerRequest();

            builder.RegisterType<HttpContextFactory>().As<IHttpContextFactory>().InstancePerRequest();

            builder.RegisterType<RazorEngineHelper>().InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<UsersService>().InstancePerRequest().PropertiesAutowired();
        }

        private static void BindServices(ContainerBuilder builder)
        {
            builder.RegisterType<FileStorageService>().As<IFileStorageService>().InstancePerRequest();
            builder.RegisterType<ListItemService>().As<IListItemService>().InstancePerRequest();
            builder.RegisterType<ListService>().As<IListService>().InstancePerRequest();
            builder.RegisterType<MailTemplateService>().As<IMailTemplateService>().InstancePerRequest();
            builder.RegisterType<MainPageImageService>().As<IMainPageImageService>().InstancePerRequest();
            builder.RegisterType<MenuService>().As<IMenuService>().InstancePerRequest();
            builder.RegisterType<ProductCategoryService>().As<IProductCategoryService>().InstancePerRequest();
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerRequest();
            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerRequest();
            builder.RegisterType<StoryCategoryService>().As<IStoryCategoryService>().InstancePerRequest();
            builder.RegisterType<StoryService>().As<IStoryService>().InstancePerRequest();
            builder.RegisterType<SubscriberService>().As<ISubscriberService>().InstancePerRequest();
            builder.RegisterType<TagCategoryService>().As<ITagCategoryService>().InstancePerRequest();
            builder.RegisterType<TagService>().As<ITagService>().InstancePerRequest();
            builder.RegisterType<TemplateService>().As<ITemplateService>().InstancePerRequest();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerRequest();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerRequest();
            builder.RegisterType<ShoppingCartService>().As<IShoppingCartService>().InstancePerRequest();
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerRequest();
            builder.RegisterType<OrderProductService>().As<IOrderProductService>().InstancePerRequest();
            builder.RegisterType<FaqService>().As<IFaqService>().InstancePerRequest();
            builder.RegisterType<ProductCommentService>().As<IProductCommentService>().InstancePerRequest();
            builder.RegisterType<BrandService>().As<IBrandService>().InstancePerRequest();
            builder.RegisterType<CouponService>().As<ICouponService>().InstancePerRequest();
        }

        private static void BindRepositories(ContainerBuilder builder)
        {
            builder.RegisterType<CouponRepository>().As<ICouponRepository>().InstancePerRequest();
            builder.RegisterType<FileStorageRepository>().As<IFileStorageRepository>().InstancePerRequest();
            builder.RegisterType<FileStorageTagRepository>().As<IFileStorageTagRepository>().InstancePerRequest();
            builder.RegisterType<ListItemRepository>().As<IListItemRepository>().InstancePerRequest();
            builder.RegisterType<ListRepository>().As<IListRepository>().InstancePerRequest();
            builder.RegisterType<MailTemplateRepository>().As<IMailTemplateRepository>().InstancePerRequest();
            builder.RegisterType<MainPageImageRepository>().As<IMainPageImageRepository>().InstancePerRequest();
            builder.RegisterType<MenuFileRepository>().As<IMenuFileRepository>().InstancePerRequest();
            builder.RegisterType<MenuRepository>().As<IMenuRepository>().InstancePerRequest();
            builder.RegisterType<ProductCategoryRepository>().As<IProductCategoryRepository>().InstancePerRequest();
            builder.RegisterType<ProductFileRepository>().As<IProductFileRepository>().InstancePerRequest();
            builder.RegisterType<ProductRepository>().As<IProductRepository>().InstancePerRequest();
            builder.RegisterType<ProductSpecificationRepository>().As<IProductSpecificationRepository>().InstancePerRequest();
            builder.RegisterType<ProductTagRepository>().As<IProductTagRepository>().InstancePerRequest();
            builder.RegisterType<SettingRepository>().As<ISettingRepository>().InstancePerRequest();
            builder.RegisterType<ShortUrlRepository>().As<IShortUrlRepository>().InstancePerRequest();
            builder.RegisterType<StoryCategoryRepository>().As<IStoryCategoryRepository>().InstancePerRequest();
            builder.RegisterType<StoryFileRepository>().As<IStoryFileRepository>().InstancePerRequest();
            builder.RegisterType<StoryRepository>().As<IStoryRepository>().InstancePerRequest();
            builder.RegisterType<StoryTagRepository>().As<IStoryTagRepository>().InstancePerRequest();
            builder.RegisterType<SubscriberRepository>().As<ISubscriberRepository>().InstancePerRequest();
            builder.RegisterType<TagCategoryRepository>().As<ITagCategoryRepository>().InstancePerRequest();
            builder.RegisterType<TagRepository>().As<ITagRepository>().InstancePerRequest();
            builder.RegisterType<TemplateRepository>().As<ITemplateRepository>().InstancePerRequest();
            builder.RegisterType<AddressRepository>().As<IAddressRepository>().InstancePerRequest();
            builder.RegisterType<CustomerRepository>().As<ICustomerRepository>().InstancePerRequest();
            builder.RegisterType<ShoppingCartRepository>().As<IShoppingCartRepository>().InstancePerRequest();
            builder.RegisterType<OrderRepository>().As<IOrderRepository>().InstancePerRequest();
            builder.RegisterType<OrderProductRepository>().As<IOrderProductRepository>().InstancePerRequest();
            builder.RegisterType<FaqRepository>().As<IFaqRepository>().InstancePerRequest();
            builder.RegisterType<ProductCommentRepository>().As<IProductCommentRepository>().InstancePerRequest();
            builder.RegisterType<BrandRepository>().As<IBrandRepository>().InstancePerRequest();
        }
    }
}
