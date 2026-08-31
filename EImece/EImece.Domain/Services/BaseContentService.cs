using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.Abstractions;
using EImece.Domain.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
        protected readonly ISettingService SettingService;
        protected readonly IFileStorageService FileStorageService;
        protected readonly ICurrentUserContext CurrentUserContext;
        protected readonly FilesHelper FilesHelper;
        public IBaseContentRepository<T> BaseContentRepository { get; }

        protected BaseContentService(
            IBaseContentRepository<T> baseContentRepository,
            IEimeceCacheProvider dataCachingProvider,
            ISettingService settingService,
            IFileStorageService fileStorageService,
            ICurrentUserContext currentUserContext,
            FilesHelper filesHelper,
            ILogger logger)
            : base(baseContentRepository, dataCachingProvider, logger)
        {
            this.BaseContentRepository = baseContentRepository;
            this.SettingService = settingService;
            this.FileStorageService = fileStorageService;
            this.CurrentUserContext = currentUserContext;
            this.FilesHelper = filesHelper;
        }

        protected BaseContentService(
            IBaseContentRepository<T> baseContentRepository,
            bool isCachingActivated,
            IEimeceCacheProvider dataCachingProvider,
            ISettingService settingService,
            IFileStorageService fileStorageService,
            ICurrentUserContext currentUserContext,
            FilesHelper filesHelper,
            ILogger logger)
            : base(baseContentRepository, isCachingActivated, dataCachingProvider, logger)
        {
            this.BaseContentRepository = baseContentRepository;
            this.SettingService = settingService;
            this.FileStorageService = fileStorageService;
            this.CurrentUserContext = currentUserContext;
            this.FilesHelper = filesHelper;
        }

        public virtual T GetBaseContent(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("Id cannot be zero");
            }
            var item = BaseContentRepository.GetBaseContent(id);
            if (item == null)
            {
                return null;
            }
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

        public virtual async Task<T> GetBaseContentAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == 0)
            {
                throw new ArgumentException("Id cannot be zero");
            }
            var item = await BaseContentRepository.GetBaseContentAsync(id, cancellationToken).ConfigureAwait(false);
            if (item == null)
            {
                return null;
            }
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
                item.ImageHeight = (await SettingService.GetSettingByKeyAsync(Constants.DefaultImageHeight).ConfigureAwait(false)).ToInt();
                item.ImageWidth = (await SettingService.GetSettingByKeyAsync(Constants.DefaultImageWidth).ConfigureAwait(false)).ToInt();
            }

            return item;
        }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            return BaseContentRepository.SearchEntities(whereLambda, search, language);
        }

        public virtual async Task<List<T>> SearchEntitiesAsync(Expression<Func<T, bool>> whereLambda, String search, int language)
        {
            return await BaseContentRepository.SearchEntitiesAsync(whereLambda, search, language).ConfigureAwait(false);
        }

        public virtual List<T> GetActiveBaseContentsFromCache(bool? isActive, int? language)
        {
            // Hierarchical key under the service's CacheKeys family so ClearByPrefix
            // invalidation (menu save, product save, ...) actually evicts it.
            String cacheKey = ActiveListCachePrefix + "activecontents:" + isActive + ":lang" + language;

            // Single-flight population coalesces concurrent misses onto one DB call.
            var result = DataCachingProvider.GetOrAdd(
                cacheKey,
                () => BaseContentRepository.GetActiveBaseContents(isActive, language),
                AppConfig.CacheLongSeconds);

            // Preserve original semantics: never persist an empty content set (so freshly added
            // content becomes visible without waiting for the long cache window). Evict and return
            // an empty list. Clear() now correctly targets the prefixed key.
            if (!result.IsNotEmpty())
            {
                DataCachingProvider.Clear(cacheKey);
                return new List<T>();
            }
            return result;
        }

        public virtual async Task<List<T>> GetActiveBaseContentsFromCacheAsync(bool? isActive, int? language)
        {
            String cacheKey = ActiveListCachePrefix + "activecontents:" + isActive + ":lang" + language + AsyncCacheKeySuffix;

            // CancellationToken.None: see AsyncCacheKeySuffix - the factory result is shared by
            // every concurrent miss, so it must not be tied to one request's lifetime.
            var result = await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => BaseContentRepository.GetActiveBaseContentsAsync(isActive, language, CancellationToken.None),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);

            // Same semantics as the synchronous overload: an empty content set is never kept, so
            // newly added content shows up without waiting out the long cache window.
            if (!result.IsNotEmpty())
            {
                DataCachingProvider.Clear(cacheKey);
                return new List<T>();
            }
            return result;
        }

        public virtual List<T> GetActiveBaseContents(bool? isActive, int? language)
        {
            return BaseContentRepository.GetActiveBaseContents(isActive, language);
        }

        public virtual async Task<List<T>> GetActiveBaseContentsAsync(bool? isActive, int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await BaseContentRepository.GetActiveBaseContentsAsync(isActive, language, cancellationToken).ConfigureAwait(false);
        }

        public virtual new T SaveOrEditEntity(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity cannot be empty");
            }

            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
                entity.UpdateUserId = CurrentUserContext?.GetCurrentUserId();
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
                entity.UpdateUserId = CurrentUserContext?.GetCurrentUserId();
                entity.AddUserId = CurrentUserContext?.GetCurrentUserId();
            }
            var tmp = BaseContentRepository.SaveOrEdit(entity);
            return entity;
        }

        public virtual new async Task<T> SaveOrEditEntityAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity cannot be empty");
            }

            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
                entity.UpdateUserId = CurrentUserContext?.GetCurrentUserId();
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
                entity.UpdateUserId = CurrentUserContext?.GetCurrentUserId();
                entity.AddUserId = CurrentUserContext?.GetCurrentUserId();
            }
            await BaseContentRepository.SaveOrEditAsync(entity).ConfigureAwait(false);
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
                Logger.LogError(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
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
                    var item = await GetBaseContentAsync(id).ConfigureAwait(false);
                    if (item.MainImageId.HasValue)
                    {
                        await FileStorageService.DeleteFileStorageAsync(item.MainImageId.Value).ConfigureAwait(false);
                    }
                    BaseContentRepository.Delete(item);
                }
                await BaseContentRepository.SaveAsync().ConfigureAwait(false);
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.LogError(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
    }
}