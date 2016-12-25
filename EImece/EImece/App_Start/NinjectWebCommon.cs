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
            var m = kernel.Bind<IEImeceContext>().To<EImeceContext>();
            m.WithConstructorArgument("nameOrConnectionString", Settings.DbConnectionKey);
            m.InRequestScope();
            kernel.Bind<IFileStorageRepository>().To<FileStorageRepository>().InRequestScope();
            kernel.Bind<IFileStorageTagRepository>().To<FileStorageTagRepository>().InRequestScope();
            kernel.Bind<IMenuRepository>().To<MenuRepository>().InRequestScope();
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

            kernel.Bind<IMenuService>().To<MenuService>().InRequestScope();
            kernel.Bind<IStoryService>().To<StoryService>().InRequestScope();
            kernel.Bind<IProductService>().To<ProductService>().InRequestScope();
            kernel.Bind<ISubsciberService>().To<SubsciberService>().InRequestScope();
            kernel.Bind<ITagService>().To<TagService>().InRequestScope();
            kernel.Bind<ITagCategoryRepository>().To<TagCategoryRepository>().InRequestScope();

        }
    }
}
