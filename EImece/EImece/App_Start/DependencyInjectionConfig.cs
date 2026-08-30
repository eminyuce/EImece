using AutoMapper;
using EImece.Domain;
using EImece.Domain.Abstractions;
using EImece.Domain.ApiRepositories;
using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Factories;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Infrastructure;
using EImece.Domain.Observability;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.HealthChecks;
using EImece.Domain.Observability.Http;
using EImece.Domain.Observability.Metrics;
using EImece.Filters;
using EImece.Domain.Observability.Telemetry;
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
        private static readonly System.Threading.AsyncLocal<IServiceScope> _ambientScope = new System.Threading.AsyncLocal<IServiceScope>();

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

            var scope = ServiceProvider.CreateScope();
            _ambientScope.Value = scope;
            return new AmbientScopeReleaser(scope);
        }

        private sealed class AmbientScopeReleaser : IDisposable
        {
            private readonly IServiceScope _scope;

            public AmbientScopeReleaser(IServiceScope scope)
            {
                _scope = scope;
            }

            public void Dispose()
            {
                if (_ambientScope.Value == _scope)
                {
                    _ambientScope.Value = null;
                }
                _scope?.Dispose();
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

            return _ambientScope.Value?.ServiceProvider;
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
            services.AddScopedWithProps<ICouponRedemptionRepository, CouponRedemptionRepository>();
            services.AddScopedWithProps<ICouponProductRepository, CouponProductRepository>();
            services.AddScopedWithProps<ICouponCategoryRepository, CouponCategoryRepository>();
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

            services.AddScopedWithProps<BitlyRepository>();
            services.AddScopedWithProps<AppLogRepository>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScopedWithProps<ICompressedImageExportService, CompressedImageExportService>();
            services.AddScopedWithProps<IImageExportRepository, ImageExportRepository>();
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
            services.AddScopedWithProps<ICouponValidationService, CouponValidationService>();
            services.AddScopedWithProps<IAppLogService, AppLogService>();
            services.AddScopedWithProps<IShortUrlService, ShortUrlService>();
            services.AddScopedWithProps<IDataExportService, DataExportService>();
            services.AddScopedWithProps<IDataExportRepository, DataExportRepository>();

            services.AddScopedWithProps<IEmailSender, EmailSender>();
            services.AddScopedWithProps<AdresService>();
            services.AddScopedWithProps<ITurkishRegionService, TurkishRegionService>();
            services.AddScopedWithProps<TurkishRegionService>();

            // Payment Strategy: Iyzico remains the default/production provider.
            // IyzicoService (Checkout Form initialize/retrieve) is unchanged and used only by IyzicoPaymentStrategy.
            services.AddScopedWithProps<IyzicoService>();
            services.AddScopedWithProps<IyzicoPaymentStrategy>();
            services.AddScoped<IPaymentStrategy>(sp =>
            {
                // Default / blank / Iyzico → keep current live payment process.
                var settingService = sp.GetService<ISettingService>();
                var provider = settingService?.GetSettingByKey(Domain.Constants.PaymentProvider) ?? Domain.Constants.DefaultPaymentProvider;
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
            services.AddScopedWithProps<IUsersService, UsersService>();

            // Domain abstraction bridges
            services.AddScoped<ICurrentUserContext, WebCurrentUserContext>();
            services.AddScoped<ISiteUrlProvider, WebSiteUrlProvider>();
            services.AddSingleton<IBackgroundWorkQueue, HostingEnvironmentBackgroundWorkQueue>();

            // Transient matches Ninject default (no scope) for IEntityFactory.
            services.AddTransientWithProps<IEntityFactory, EntityFactory>();
        }

        private static void RegisterHelpers(IServiceCollection services)
        {
            services.AddScopedWithProps<FilesHelper>();
            services.AddScopedWithProps<XmlEditorHelper>();
            services.AddScopedWithProps<IRazorEngineHelper, RazorEngineHelper>();
            services.AddScopedWithProps<RazorEngineHelper>();
        }

        private static void RegisterIdentity(IServiceCollection services)
        {
            services.AddScoped<ApplicationDbContext>();
            services.AddScopedWithProps<IIdentityManager, IdentityManager>();
            services.AddScopedWithProps<ApplicationUserManager>();
            services.AddScopedWithProps<ApplicationSignInManager>();
            services.AddScopedWithProps<ITwoFactorTokenRepository, TwoFactorTokenRepository>();
            services.AddScopedWithProps<IUserRepository, UserRepository>();
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
            services.AddScoped<UserStore<ApplicationUser>>(sp =>
                new UserStore<ApplicationUser>(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IRoleStore<IdentityRole, string>>(sp =>
                new RoleStore<IdentityRole>(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<RoleStore<IdentityRole>>(sp =>
                new RoleStore<IdentityRole>(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<RoleManager<IdentityRole>>();

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

    public static class ServiceCollectionPropertyInjectionExtensions
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
            services.AddSingleton<TImplementation>(sp =>
            {
                var instance = PropertyInjector.Create<TImplementation>(sp);
                return MaybeWrapConcreteWithTimedInterceptor(instance);
            });
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
            services.AddScoped<TImplementation>(sp =>
            {
                var instance = PropertyInjector.Create<TImplementation>(sp);
                return MaybeWrapConcreteWithTimedInterceptor(instance);
            });
        }

        public static void AddTransientWithProps<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TImplementation>(sp => PropertyInjector.Create<TImplementation>(sp));
            services.AddTransient<TService>(ResolveImplementationOrUnderConstruction<TService, TImplementation>);
        }

        public static void AddTransientWithProps<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            services.AddTransient<TImplementation>(sp =>
            {
                var instance = PropertyInjector.Create<TImplementation>(sp);
                return MaybeWrapConcreteWithTimedInterceptor(instance);
            });
        }

        /// <summary>
        /// Shares the concrete scoped/singleton instance for the interface registration.
        /// When the concrete is mid-construction (circular [Inject] graph), returns that
        /// in-flight instance instead of re-entering MS.DI's scope cache.
        /// After construction, interface resolutions are wrapped with <see cref="TimedInterceptor"/>
        /// when any method carries [TimedAttribute] (service.{entity}.{operation} / repo.{entity}.{operation}),
        /// and with <see cref="MeasuredServiceProxy"/> when service-method metrics are enabled.
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
            var timed = MaybeWrapWithTimedInterceptor<TService, TImplementation>(implementation, sp);
            return MaybeWrapWithMetricsProxy<TService>(timed, sp);
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

        private static TService MaybeWrapWithTimedInterceptor<TService, TImplementation>(TService instance, IServiceProvider sp)
            where TService : class
            where TImplementation : class, TService
        {
            if (instance == null)
                return instance;

            // Only wrap when any method on the interface or implementation carries [TimedAttribute].
            // Covers both service.{entity}.{operation} and repo.{entity}.{operation}.
            var hasTimed = HasTimedAttribute(typeof(TImplementation)) || HasTimedAttribute(typeof(TService));
            if (!hasTimed)
                return instance;

            try
            {
                // Prefer interface proxy (no virtual requirement); class proxy fallback.
                if (typeof(TService).IsInterface)
                    return ProxyFactory.CreateInterface<TService>(instance);

                return ProxyFactory.Create<TService>(instance);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MaybeWrapWithTimedInterceptor failed for {typeof(TService).Name}: {ex}");
                return instance;
            }
        }

        private static TImplementation MaybeWrapConcreteWithTimedInterceptor<TImplementation>(TImplementation instance)
            where TImplementation : class
        {
            if (instance == null)
                return instance;

            if (!HasTimedAttribute(typeof(TImplementation)))
                return instance;

            try
            {
                return ProxyFactory.Create<TImplementation>(instance);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MaybeWrapConcreteWithTimedInterceptor failed for {typeof(TImplementation).Name}: {ex}");
                return instance;
            }
        }

        private static bool HasTimedAttribute(Type type)
        {
            if (type == null)
                return false;
            try
            {
                var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    // Check method itself and its base definition (handles interface mapping).
                    if (Attribute.IsDefined(m, typeof(TimedAttribute), true))
                        return true;
                    var baseDef = m.GetBaseDefinition();
                    if (baseDef != null && baseDef != m && Attribute.IsDefined(baseDef, typeof(TimedAttribute), true))
                        return true;
                }

                // Also check interfaces implemented by the type for Timed attributes.
                var interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    var ifaceMethods = interfaces[i].GetMethods();
                    for (int j = 0; j < ifaceMethods.Length; j++)
                    {
                        if (Attribute.IsDefined(ifaceMethods[j], typeof(TimedAttribute), true))
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasTimedAttribute check failed for {type.Name}: {ex}");
            }
            return false;
        }
    }
}
