using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq.Expressions;

namespace EImece.Domain.Services
{
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
        private static readonly Logger BaseContentServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ITagCategoryService TagCategoryService { get; set; }

        [Inject]
        public ISettingService SettingService { get; set; }

        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        [Inject]
        public IMenuService MenuService { get; set; }

        public IBaseContentRepository<T> BaseContentRepository { get; set; }

        protected BaseContentService(IBaseContentRepository<T> baseContentRepository) : base(baseContentRepository)
        {
            this.BaseContentRepository = baseContentRepository;
        }
        protected BaseContentService(IBaseContentRepository<T> baseContentRepository, bool IsCachingActivated) : base(baseContentRepository)
        {
            this.IsCachingActivated = IsCachingActivated;
        }
        public virtual T GetBaseContent(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("Id cannot be zero");
            }
            var item = BaseContentRepository.GetBaseContent(id);
            if (item.MainImageId.HasValue && item.MainImageId > 0)
            {
                var imageSize = FilesHelper.GetThumbnailImageSize(item.MainImage);
                item.ImageHeight = imageSize.ThumpBitmapHeight;
                item.ImageWidth = imageSize.ThumpBitmapWidth;
                if (item.MainImage != null)
                    item.MainImageId = item.MainImage.Id;
            }
            else
            {
                item.ImageHeight = SettingService.GetSettingByKey(Constants.DefaultImageHeight).ToInt();
                item.ImageWidth = SettingService.GetSettingByKey(Constants.DefaultImageWidth).ToInt();
            }

            return item;
        }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            return BaseContentRepository.SearchEntities(whereLambda, search, language);
        }

        public virtual List<T> GetActiveBaseContentsFromCache(bool? isActive, int? language)
        {
            List<T> result = null;
            String cacheKey = String.Format(this.GetType().FullName + "-GetActiveBaseContentsFromCache-{0}-{1}", isActive, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = BaseContentRepository.GetActiveBaseContents(isActive, language);
                if (result.IsNotEmpty())
                {
                    MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
                }
                else
                {
                    return new List<T>();
                }
            }
            return result;
        }

        public virtual List<T> GetActiveBaseContents(bool? isActive, int? language)
        {
            return BaseContentRepository.GetActiveBaseContents(isActive, language);
        }

        public new virtual T SaveOrEditEntity(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity cannot be empty");
            }

            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
                entity.UpdateUserId = HttpContextFactory.GetCurrentUserId();
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
                entity.UpdateUserId = HttpContextFactory.GetCurrentUserId();
                entity.AddUserId = HttpContextFactory.GetCurrentUserId();
            }
            var tmp = BaseContentRepository.SaveOrEdit(entity);
            this.MemoryCacheProvider.ClearAll();
            return entity;
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            if (values.IsEmpty())
            {
                throw new ArgumentException("List cannot be empty");
            }
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    var item = GetBaseContent(id);
                    if (item.MainImageId.HasValue)
                    {
                        FileStorageService.DeleteFileStorage(item.MainImageId.Value);
                    }
                    BaseContentRepository.Delete(item);
                }
                BaseContentRepository.Save();
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                BaseContentServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                BaseContentServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
    }
}