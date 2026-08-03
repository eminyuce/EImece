using AutoMapper;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public abstract class BaseService<T> where T : class, IEntity<int>
    {
        [Inject]
        public IHttpContextFactory HttpContextFactory { get; set; }
        [Inject]
        public IMapper Mapper { get; set; }

        private IEimeceCacheProvider _dataCacheProvider { get; set; }

        public bool IsCachingActivated { get; set; } = true;

        [Inject]
        public IEimeceCacheProvider DataCachingProvider
        {
            get
            {
                return _dataCacheProvider;
            }
            set
            {
                _dataCacheProvider = value;
            }
        }

        private IBaseRepository<T> baseRepository { get; set; }

        protected BaseService(IBaseRepository<T> baseRepository)
        {
            this.baseRepository = baseRepository;
        }

        protected BaseService(IBaseRepository<T> baseRepository, bool IsCachingActivated)
        {
            this.baseRepository = baseRepository;
            this.IsCachingActivated = IsCachingActivated;
        }

        public virtual List<T> LoadEntites(Expression<Func<T, bool>> whereLambda)
        {
            return baseRepository.FindBy(whereLambda).ToList();
        }

        public virtual List<T> GetAll()
        {
            return baseRepository.GetAll().ToList();
        }

        public virtual T GetSingle(int id)
        {
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

        public virtual bool DeleteById(int id)
        {
            return this.baseRepository.DeleteByWhereCondition(r => r.Id == id);
        }

        public virtual void DeleteBaseEntity(List<string> values)
        {
            baseRepository.DeleteBaseEntity(values);
        }

        public virtual async Task<T> GetSingleAsync(int id)
        {
            return await baseRepository.GetSingleAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<T> SaveOrEditEntityAsync(T entity)
        {
            await baseRepository.SaveOrEditAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await baseRepository.GetAll().ToListAsync().ConfigureAwait(false);
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
                _filesHelper.InitFilesMediaFolder();
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