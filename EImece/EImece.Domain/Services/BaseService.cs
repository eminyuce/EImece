using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using System.Linq.Expressions;
using GenericRepository.EntityFramework;
using System.Data.Entity.Infrastructure;
using Ninject;
using EImece.Domain.Helpers;
using System.Data.Entity.Validation;
using NLog;

namespace EImece.Domain.Services
{
    public abstract class BaseService<T> where T : class, IEntity<int>
    {
        protected static readonly Logger BaseServiceLogger = LogManager.GetCurrentClassLogger();


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
        public virtual void DeleteBaseEntity(List<string> values)
        {
            baseRepository.DeleteBaseEntity(values);
        }

        [Inject]
        public IProductTagRepository ProductTagRepository { get; set; }
        [Inject]
        public IFileStorageTagRepository FileStorageTagRepository { get; set; }
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

        private FilesHelper _filesHelper { get; set; }
        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.Init(Settings.DeleteURL, Settings.DeleteType, Settings.StorageRoot, Settings.UrlBase, Settings.TempPath, Settings.ServerMapPath);
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }
    }
}
