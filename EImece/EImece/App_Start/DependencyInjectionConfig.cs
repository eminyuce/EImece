using AutoMapper;
using EImece.Domain;
using EImece.Domain.ApiRepositories;
using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Factories;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Observability;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.HealthChecks;
using EImece.Domain.Observability.Http;
using EImece.Domain.Observability.Metrics;
using EImece.Filters;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.ExportImport;
using EImece.Domain.Services.IServices;
using EImece.Domain.Services.Payment;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Security;
using NLog.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using EImece.Domain.Scheduler;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace EImece.App_Start
{
    /// <summary>
    /// Composition root: Microsoft.Extensions.DependencyInjection replaces Ninject.
    /// </summary>
    public static class DependencyInjectionConfig
    {
        private const string RequestScopeKey = "EImece.MsDi.RequestScope";

        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Optional ambient scope for Application_Start (no HttpContext) when resolving scoped services.
        /// </summary>
        private static IServiceScope _ambientScope;

        /// <summary>
        /// Called early from Application_Start (after ConnectionStringProvider.Initialize).
        /// </summary>
        public static void Register()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider(validateScopes: true);
            DomainServiceProvider.Instance = ServiceProvider;

            DependencyResolver.SetResolver(new MsDiDependencyResolver(ServiceProvider));
            GlobalConfiguration.Configuration.DependencyResolver =
                new MsDiWebApiDependencyResolver(ServiceProvider);
        }

        /// <summary>
        /// Creates an ambient DI scope for startup code that runs without HttpContext.
        /// </summary>
        public static IDisposable BeginAmbientScope()
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException("DependencyInjectionConfig.Register() must be called first.");
            }

            SetAmbientScope(ServiceProvider.CreateScope());
            return new AmbientScopeReleaser();
        }

        private static void SetAmbientScope(IServiceScope scope)
        {
            _ambientScope = scope;
        }

        private sealed class AmbientScopeReleaser : IDisposable
        {
            public void Dispose()
            {
                _ambientScope?.Dispose();
                _ambientScope = null;
            }
        }

        public static void BeginRequestScope()
        {
            if (ServiceProvider == null || HttpContext.Current == null)
            {
                return;
            }

            if (HttpContext.Current.Items[RequestScopeKey] != null)
            {
                return;
            }

            HttpContext.Current.Items[RequestScopeKey] = ServiceProvider.CreateScope();
        }

        public static void EndRequestScope()
        {
            if (HttpContext.Current?.Items[RequestScopeKey] is IServiceScope scope)
            {
                HttpContext.Current.Items.Remove(RequestScopeKey);
                scope.Dispose();
            }
        }

        public static IServiceProvider GetRequestServiceProvider()
        {
            if (HttpContext.Current?.Items[RequestScopeKey] is IServiceScope scope)
            {
                return scope.ServiceProvider;
            }

            return _ambientScope?.ServiceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            RegisterCaching(services);
            RegisterObservability(services);
            RegisterLogging(services);
            RegisterAutoMapper(services);
            RegisterData(services);
            RegisterRepositories(services);
            RegisterServices(services);
            RegisterHelpers(services);
            RegisterIdentity(services);
            RegisterScheduler(services);
        }

        private static void RegisterCaching(IServiceCollection services)
        {
            services.AddSingletonWithProps<IEimeceCacheProvider, LazyCacheProvider>();
            // Single, compile-once Razor engine shared across the app (thread-safe).
            services.AddSingletonWithProps<IRazorTemplateEngine, RazorTemplateEngine>();
        }

        private static void RegisterObservability(IServiceCollection services)
        {
            services.AddSingleton(_ => ObservabilityOptions.FromAppConfig());
            services.AddSingletonWithProps<IApplicationMetrics, ApplicationMetrics>();
            services.AddSingletonWithProps<IResilientHttpClient, ResilientHttpClient>();
            // Async, resilient image downloader — DI replacement for the removed static accessor.
            services.AddSingletonWithProps<IImageDownloadService, ImageDownloadService>();

            // Telemetry ActionFilter is also added globally in Application_Start; register for DI resolution.
            services.AddSingleton<TelemetryActionFilter>(sp =>
                new TelemetryActionFilter(
                    sp.GetRequiredService<IApplicationMetrics>(),
                    sp.GetRequiredService<ObservabilityOptions>()));

            // Multiple IHealthCheck implementations — GetServices / IEnumerable<IHealthCheck> returns all.
            services.AddSingleton<IHealthCheck>(sp => PropertyInjector.Create<SqlServerHealthCheck>(sp));
            services.AddSingleton<IHealthCheck>(sp => PropertyInjector.Create<FileStorageHealthCheck>(sp));
            services.AddSingleton<IHealthCheck>(sp => PropertyInjector.Create<BackgroundServiceHealthCheck>(sp));
            services.AddSingletonWithProps<IHealthCheckService, HealthCheckService>();

            // OpenTelemetry providers are initialized once from ObservabilityBootstrap.Configure().
            services.AddSingleton(sp =>
                OpenTelemetryBootstrap.Initialize(sp.GetRequiredService<ObservabilityOptions>()));
        }

        private static void RegisterLogging(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory>(_ =>
            {
                var factory = new LoggerFactory();
                factory.AddProvider(new NLogLoggerProvider());
                return factory;
            });

            // Open-generic ILogger<T> (covers ResilientHttpClient and any other consumers).
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

        private static void RegisterAutoMapper(IServiceCollection services)
        {
            services.AddSingleton(sp => CreateAutoMapper(sp.GetRequiredService<ILoggerFactory>()));
            // Mirror Ninject: IMapper is request-scoped from the shared MapperConfiguration.
            services.AddScoped<IMapper>(sp => sp.GetRequiredService<MapperConfiguration>().CreateMapper());
        }

        private static MapperConfiguration CreateAutoMapper(ILoggerFactory loggerFactory)
        {
            // AutoMapper 15+ requires ILoggerFactory for diagnostics
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            }, loggerFactory);

#if DEBUG
            config.AssertConfigurationIsValid();
#endif
            return config;
        }

        private static void RegisterData(IServiceCollection services)
        {
            // Mirror Ninject WithConstructorArgument("nameOrConnectionString", ...).InRequestScope()
            services.AddScoped<IEImeceContext>(_ =>
                new EImeceContext(ConnectionStringProvider.GetConnectionString()));
            services.AddScoped(sp => (EImeceContext)sp.GetRequiredService<IEImeceContext>());
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScopedWithProps<ICouponRepository, CouponRepository>();
            services.AddScopedWithProps<IFileStorageRepository, FileStorageRepository>();
            services.AddScopedWithProps<IFileStorageTagRepository, FileStorageTagRepository>();
            services.AddScopedWithProps<IListItemRepository, ListItemRepository>();
            services.AddScopedWithProps<IListRepository, ListRepository>();
            services.AddScopedWithProps<IMailTemplateRepository, MailTemplateRepository>();
            services.AddScopedWithProps<IMainPageImageRepository, MainPageImageRepository>();
            services.AddScopedWithProps<IMenuFileRepository, MenuFileRepository>();
            services.AddScopedWithProps<IMenuRepository, MenuRepository>();
            services.AddScopedWithProps<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScopedWithProps<IProductFileRepository, ProductFileRepository>();
            services.AddScopedWithProps<IProductRepository, ProductRepository>();
            services.AddScopedWithProps<IProductSpecificationRepository, ProductSpecificationRepository>();
            services.AddScopedWithProps<IProductTagRepository, ProductTagRepository>();
            services.AddScopedWithProps<ISettingRepository, SettingRepository>();
            services.AddScopedWithProps<IShortUrlRepository, ShortUrlRepository>();
            services.AddScopedWithProps<IStoryCategoryRepository, StoryCategoryRepository>();
            services.AddScopedWithProps<IStoryFileRepository, StoryFileRepository>();
            services.AddScopedWithProps<IStoryRepository, StoryRepository>();
            services.AddScopedWithProps<IStoryTagRepository, StoryTagRepository>();
            services.AddScopedWithProps<ISubscriberRepository, SubscriberRepository>();
            services.AddScopedWithProps<ITagCategoryRepository, TagCategoryRepository>();
            services.AddScopedWithProps<ITagRepository, TagRepository>();
            services.AddScopedWithProps<ITemplateRepository, TemplateRepository>();
            services.AddScopedWithProps<IAddressRepository, AddressRepository>();
            services.AddScopedWithProps<ICustomerRepository, CustomerRepository>();
            services.AddScopedWithProps<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddScopedWithProps<IOrderRepository, OrderRepository>();
            services.AddScopedWithProps<IOrderProductRepository, OrderProductRepository>();
            services.AddScopedWithProps<IFaqRepository, FaqRepository>();
            services.AddScopedWithProps<IProductCommentRepository, ProductCommentRepository>();
            services.AddScopedWithProps<IBrandRepository, BrandRepository>();

            services.AddScopedWithProps<MigrationRepository>();
            services.AddScopedWithProps<BitlyRepository>();
            services.AddScopedWithProps<AppLogRepository>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScopedWithProps<ICompressedImageExportService, CompressedImageExportService>();
            services.AddScopedWithProps<IFileStorageService, FileStorageService>();
            services.AddScopedWithProps<IListItemService, ListItemService>();
            services.AddScopedWithProps<IListService, ListService>();
            services.AddScopedWithProps<IMailTemplateService, MailTemplateService>();
            services.AddScopedWithProps<IMailTemplateTestService, MailTemplateTestService>();
            services.AddScopedWithProps<IMainPageImageService, MainPageImageService>();
            services.AddScopedWithProps<IMenuService, MenuService>();
            services.AddScopedWithProps<IProductCategoryService, ProductCategoryService>();
            services.AddScopedWithProps<IProductService, ProductService>();
            services.AddScopedWithProps<ISettingService, SettingService>();
            services.AddScopedWithProps<IWebAppManifestService, WebAppManifestService>();
            services.AddScopedWithProps<IStoryCategoryService, StoryCategoryService>();
            services.AddScopedWithProps<IStoryService, StoryService>();
            services.AddScopedWithProps<ISubscriberService, SubscriberService>();
            services.AddScopedWithProps<ITagCategoryService, TagCategoryService>();
            services.AddScopedWithProps<ITagService, TagService>();
            services.AddScopedWithProps<ITemplateService, TemplateService>();
            services.AddScopedWithProps<IAddressService, AddressService>();
            services.AddScopedWithProps<ICustomerService, CustomerService>();
            services.AddScopedWithProps<IShoppingCartService, ShoppingCartService>();
            services.AddScopedWithProps<IOrderService, OrderService>();
            services.AddScopedWithProps<IOrderProductService, OrderProductService>();
            services.AddScopedWithProps<IFaqService, FaqService>();
            services.AddScopedWithProps<IProductCommentService, ProductCommentService>();
            services.AddScopedWithProps<IBrandService, BrandService>();
            services.AddScopedWithProps<ICouponService, CouponService>();
            services.AddScopedWithProps<IDataExportService, DataExportService>();

            services.AddScopedWithProps<IEmailSender, EmailSender>();
            services.AddScopedWithProps<AdresService>();

            // Payment Strategy: Iyzico remains the default/production provider.
            // IyzicoService (Checkout Form initialize/retrieve) is unchanged and used only by IyzicoPaymentStrategy.
            services.AddScopedWithProps<IyzicoService>();
            services.AddScopedWithProps<IyzicoPaymentStrategy>();
            services.AddScoped<IPaymentStrategy>(sp =>
            {
                // Default / blank / Iyzico → keep current live payment process.
                var provider = AppConfig.PaymentProvider;
                if (string.IsNullOrWhiteSpace(provider)
                    || string.Equals(provider, "Iyzico", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetRequiredService<IyzicoPaymentStrategy>();
                }

                throw new InvalidOperationException(
                    "Unsupported PaymentProvider '" + provider + "'. "
                    + "Keep PaymentProvider=Iyzico for production, or register a matching IPaymentStrategy.");
            });
            services.AddScoped<PaymentContext>(sp =>
                new PaymentContext(sp.GetRequiredService<IPaymentStrategy>()));

            services.AddScopedWithProps<ReportService>();
            services.AddScopedWithProps<SiteMapService>();
            services.AddScopedWithProps<UsersService>();

            // Transient matches Ninject default (no scope) for IEntityFactory / IHttpContextFactory.
            services.AddTransientWithProps<IEntityFactory, EntityFactory>();
            services.AddTransientWithProps<IHttpContextFactory, HttpContextFactory>();
        }

        private static void RegisterHelpers(IServiceCollection services)
        {
            services.AddScopedWithProps<FilesHelper>();
            services.AddScopedWithProps<XmlEditorHelper>();
            services.AddScopedWithProps<RazorEngineHelper>();
        }

        private static void RegisterIdentity(IServiceCollection services)
        {
            services.AddScopedWithProps<IdentityManager>();
            services.AddScopedWithProps<ApplicationDbContext>();
            services.AddScopedWithProps<ApplicationUserManager>();
            services.AddScopedWithProps<ApplicationSignInManager>();
            services.AddScopedWithProps<TwoFactorTokenService>();

            // Former named Ninject bindings for IIdentityMessageService ("Email"/"Sms"):
            // register concretes; ApplicationUserManager takes EmailService + SmsService.
            services.AddScopedWithProps<EmailService>();
            services.AddScopedWithProps<SmsService>();
            services.AddScoped<Func<string, IIdentityMessageService>>(sp => name =>
            {
                if (string.Equals(name, "Sms", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetRequiredService<SmsService>();
                }

                if (string.Equals(name, "Email", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetRequiredService<EmailService>();
                }

                throw new ArgumentException("Unknown IIdentityMessageService name: " + name, nameof(name));
            });

            services.AddScoped(sp => new IdentityFactoryOptions<ApplicationUserManager>
            {
                DataProtectionProvider = Startup.DataProtectionProvider
            });

            services.AddScoped<IUserStore<ApplicationUser>>(sp =>
                new UserStore<ApplicationUser>(sp.GetRequiredService<ApplicationDbContext>()));

            services.AddScoped(sp =>
            {
                var context = HttpContext.Current;
                if (context == null)
                {
                    throw new InvalidOperationException(
                        "IAuthenticationManager requires HttpContext.Current (request scope).");
                }

                return context.GetOwinContext().Authentication;
            });
        }

        private static void RegisterScheduler(IServiceCollection services)
        {
            // Resolve once at startup via GetAwaiter().GetResult() — same as Ninject singleton binding.
            services.AddSingleton<IScheduler>(_ =>
                new StdSchedulerFactory().GetScheduler().GetAwaiter().GetResult());
            services.AddSingletonWithProps<AdminQuartzService>();
            services.AddSingletonWithProps<UserQuartzService>();
            services.AddSingletonWithProps<QuartzService>();
        }

    }

    internal static class ServiceCollectionPropertyInjectionExtensions
    {
        public static void AddSingletonWithProps<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TImplementation>(sp => PropertyInjector.Create<TImplementation>(sp));
            services.AddSingleton<TService>(ResolveImplementationOrUnderConstruction<TService, TImplementation>);
        }

        public static void AddSingletonWithProps<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            services.AddSingleton(sp => PropertyInjector.Create<TImplementation>(sp));
        }

        public static void AddScopedWithProps<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddScoped<TImplementation>(sp => PropertyInjector.Create<TImplementation>(sp));
            services.AddScoped<TService>(ResolveImplementationOrUnderConstruction<TService, TImplementation>);
        }

        public static void AddScopedWithProps<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            services.AddScoped(sp => PropertyInjector.Create<TImplementation>(sp));
        }

        public static void AddTransientWithProps<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TImplementation>(sp => PropertyInjector.Create<TImplementation>(sp));
            services.AddTransient<TService>(ResolveImplementationOrUnderConstruction<TService, TImplementation>);
        }

        /// <summary>
        /// Shares the concrete scoped/singleton instance for the interface registration.
        /// When the concrete is mid-construction (circular [Inject] graph), returns that
        /// in-flight instance instead of re-entering MS.DI's scope cache.
        /// After construction, interface resolutions are wrapped with <see cref="MeasuredServiceProxy"/>
        /// when service-method metrics are enabled.
        /// </summary>
        private static TService ResolveImplementationOrUnderConstruction<TService, TImplementation>(IServiceProvider sp)
            where TService : class
            where TImplementation : class, TService
        {
            var underConstruction = PropertyInjector.TryGetUnderConstruction(typeof(TImplementation))
                ?? PropertyInjector.TryGetUnderConstruction(typeof(TService));
            if (underConstruction is TService typed)
            {
                // During circular [Inject] graphs, return the bare instance (not a proxy).
                return typed;
            }

            var implementation = sp.GetRequiredService<TImplementation>();
            return MaybeWrapWithMetricsProxy<TService>(implementation, sp);
        }

        private static TService MaybeWrapWithMetricsProxy<TService>(TService implementation, IServiceProvider sp)
            where TService : class
        {
            if (implementation == null || !ShouldMeasureAsService(typeof(TService)))
            {
                return implementation;
            }

            var options = sp.GetService<ObservabilityOptions>();
            if (options == null || !options.EnableMetrics || !options.EnableServiceMethodMetrics)
            {
                return implementation;
            }

            var metrics = sp.GetService<IApplicationMetrics>();
            if (metrics == null)
            {
                return implementation;
            }

            return MeasuredServiceProxy.Create(implementation, metrics);
        }

        /// <summary>
        /// Only interface-based application Services are proxied — not repositories, factories, or caches.
        /// </summary>
        private static bool ShouldMeasureAsService(Type serviceType)
        {
            if (serviceType == null || !serviceType.IsInterface)
            {
                return false;
            }

            var ns = serviceType.Namespace ?? string.Empty;
            if (ns.IndexOf(".Services.IServices", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            // Catch helpers registered as *Service (e.g. IImageDownloadService) without pulling in repositories.
            var name = serviceType.Name;
            return name.EndsWith("Service", StringComparison.Ordinal)
                && name.IndexOf("Repository", StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}
