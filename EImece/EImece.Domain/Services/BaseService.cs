using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Services
{
    public abstract class BaseService<T> where T : class, IEntity<int>
    {
        [Inject]
        public IHttpContextFactory HttpContextFactory { get; set; }

        private static readonly Logger BaseServiceLogger = LogManager.GetCurrentClassLogger();
        public bool IsCachingActive { get; set; }
        private ICacheProvider _memoryCacheProvider { get; set; }

        [Inject]
        public ICacheProvider MemoryCacheProvider
        {
            get
            {
                _memoryCacheProvider.IsCacheProviderActive = IsCachingActive;
                return _memoryCacheProvider;
            }
            set
            {
                _memoryCacheProvider = value;
            }
        }

        private IBaseRepository<T> baseRepository { get; set; }

        protected BaseService(IBaseRepository<T> baseRepository)
        {
            this.baseRepository = baseRepository;
        }

        public virtual List<T> LoadEntites(Expression<Func<T, bool>> whereLambda)
        {
            return baseRepository.FindBy(whereLambda).ToList();
        }

        public virtual List<T> GetAll()
        {
            // Get call stack
            StackTrace stackTrace = new StackTrace();
            // Get calling method name
            Console.WriteLine("GetAll  " + stackTrace.GetFrame(1).GetMethod().Name);
            return baseRepository.GetAll().ToList();
        }

        public virtual T GetSingle(int id)
        {
            // Get call stack
            StackTrace stackTrace = new StackTrace();
            // Get calling method name
            Console.WriteLine("GetSingle  " + stackTrace.GetFrame(1).GetMethod().Name);
            return baseRepository.GetSingle(id);
        }

        public virtual T SaveOrEditEntity(T entity)
        {
            var tmp = baseRepository.SaveOrEdit(entity);
            return entity;
        }

        public virtual bool DeleteEntity(T entity)
        {
            var result = this.baseRepository.DeleteItem(entity);
            return result == 1;
        }

        public virtual void DeleteBaseEntity(List<string> values)
        {
            baseRepository.DeleteBaseEntity(values);
        }

        [Inject]
        public IProductTagRepository ProductTagRepository { get; set; }

        [Inject]
        public IStoryTagRepository StoryTagRepository { get; set; }

        [Inject]
        public IStoryFileRepository StoryFileRepository { get; set; }

        [Inject]
        public IProductFileRepository ProductFileRepository { get; set; }

        [Inject]
        public IMenuFileRepository MenuFileRepository { get; set; }

        [Inject]
        public IProductSpecificationRepository ProductSpecificationRepository { get; set; }

        [Inject]
        public IFileStorageTagRepository FileStorageTagRepository { get; set; }

        private FilesHelper _filesHelper { get; set; }

        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.Init(ApplicationConfigs.DeleteURL, ApplicationConfigs.DeleteType, ApplicationConfigs.StorageRoot, ApplicationConfigs.UrlBase, ApplicationConfigs.TempPath, ApplicationConfigs.ServerMapPath);
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }

        [Inject]
        public IEntityFactory EntityFactory { get; set; }
    }
}