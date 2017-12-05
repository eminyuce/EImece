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
    using System.Security.Principal;
    using System.Reflection;
    using System.Linq;

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
            var baseRepoTypes = Assembly.GetAssembly(typeof(IBaseRepository<>)).GetTypes().Where(t => t.Name.Contains("Repository")).ToList();
            foreach (var type in baseRepoTypes)
            {
                var interfaceType = type.GetInterface("I" + type.Name);
                if (interfaceType != null)
                {
                     kernel.Bind(interfaceType).To(type).InRequestScope();
                }
            }
            var baseServiceTypes = Assembly.GetAssembly(typeof(IBaseService<>))
                .GetTypes().Where(t => t.Name.Contains("Service")).ToList();

            foreach (var type in baseServiceTypes)
            {
                var intefa = type.GetInterfaces();
                var isClass = type.IsClass;
                var IsAbstract = type.IsAbstract;
                var co = intefa.Any(t => t.Name
                .Equals(typeof(IBaseEntityService<>).Name,
                StringComparison.InvariantCultureIgnoreCase));
                if (isClass && !IsAbstract && co)
                {
                    var interfaceType = type.GetInterface("I" + type.Name);
                    if (interfaceType != null)
                    {
                        kernel.Bind(interfaceType).To(type).InRequestScope();
                    }
                }

            }
      

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

            kernel.Bind<IHttpContextFactory>().To<HttpContextFactory>();

            kernel.Bind<RazorEngineHelper>().ToSelf().InRequestScope();

        }        
    }
}
