using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class FileStorageService : BaseEntityService<FileStorage>, IFileStorageService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IFileStorageRepository FileStorageRepository { get; }
        private readonly IProductFileRepository ProductFileRepository;
        private readonly IStoryFileRepository StoryFileRepository;
        private readonly IMenuFileRepository MenuFileRepository;
        private readonly IFileStorageTagRepository FileStorageTagRepository;

        public FileStorageService(
            IFileStorageRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            IProductFileRepository productFileRepository,
            IStoryFileRepository storyFileRepository,
            IMenuFileRepository menuFileRepository,
            IFileStorageTagRepository fileStorageTagRepository)
            : base(repository, dataCachingProvider)
        {
            FileStorageRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            ProductFileRepository = productFileRepository ?? throw new ArgumentNullException(nameof(productFileRepository));
            StoryFileRepository = storyFileRepository ?? throw new ArgumentNullException(nameof(storyFileRepository));
            MenuFileRepository = menuFileRepository ?? throw new ArgumentNullException(nameof(menuFileRepository));
            FileStorageTagRepository = fileStorageTagRepository ?? throw new ArgumentNullException(nameof(fileStorageTagRepository));
        }

        private static bool PhysicalFileExists(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string fullPath = SecurityHelper.GetSafeStorageFilePath(AppConfig.StorageRoot, fileName);
            return System.IO.File.Exists(fullPath);
        }

        private static void TryDeletePhysicalFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.Equals(fileName, FilesHelper.EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            try
            {
                string fullPath = SecurityHelper.GetSafeStorageFilePath(AppConfig.StorageRoot, fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TryDeletePhysicalFile failed for {0}", fileName);
            }
        }

        public FileStorage GetFileStorage(int fileStorageId)
        {
            if (fileStorageId <= 0) return null;
            if (!IsCachingActivated)
            {
                return FileStorageRepository.GetAll().AsNoTracking().FirstOrDefault(r => r.Id == fileStorageId) ?? GetSingle(fileStorageId);
            }
            var cacheKey = $"FileStorage:{fileStorageId}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => FileStorageRepository.GetAll().AsNoTracking().FirstOrDefault(r => r.Id == fileStorageId) ?? GetSingle(fileStorageId),
                AppConfig.CacheMediumSeconds);
        }

        public async Task<FileStorage> GetFileStorageAsync(int fileStorageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (fileStorageId <= 0) return null;
            if (!IsCachingActivated)
            {
                var direct = await FileStorageRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(r => r.Id == fileStorageId, cancellationToken).ConfigureAwait(false);
                return direct ?? await GetSingleAsync(fileStorageId).ConfigureAwait(false);
            }
            var cacheKey = $"FileStorage:{fileStorageId}" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                async () => await FileStorageRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(r => r.Id == fileStorageId, CancellationToken.None).ConfigureAwait(false) ?? await GetSingleAsync(fileStorageId).ConfigureAwait(false),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public List<FileStorage> GetFileStorages()
        {
            // FIX (pre-existing bug): the previous get-then-set logic fell into the else branch on a
            // cache HIT and re-queried the database every call, so the cache was effectively dead.
            // Now caching actually serves from cache, with single-flight population on a miss.
            if (!IsCachingActivated)
            {
                return FileStorageRepository.GetAll().ToList();
            }

            var cacheKey = "GetFileStorages";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => FileStorageRepository.GetAll().ToList(),
                AppConfig.CacheMediumSeconds);
        }

        public async Task<List<FileStorage>> GetFileStoragesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsCachingActivated)
            {
                return await FileStorageRepository.GetAll().ToListAsync(cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = "GetFileStorages" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => FileStorageRepository.GetAll().ToListAsync(CancellationToken.None),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public void SaveUploadImages(int contentId,
            EImeceImageType? contentImageType,
            MediaModType? contentMediaType,
            List<ViewDataUploadFilesResult> resultList,
            int language,
            string selectedTags
            )
        {
            foreach (var file in resultList)
            {
                try
                {
                    var fileStorage = new FileStorage();
                    fileStorage.Name = file.name;
                    fileStorage.FileName = file.name;
                    fileStorage.Width = file.width;
                    fileStorage.Height = file.height;
                    fileStorage.MimeType = file.mimeType;
                    fileStorage.CreatedDate = DateTime.Now;
                    fileStorage.UpdatedDate = DateTime.Now;
                    fileStorage.IsActive = true;
                    fileStorage.Position = 1;
                    fileStorage.FileSize = file.size;
                    fileStorage.IsFileExist = PhysicalFileExists(fileStorage.FileName);
                    fileStorage.Type = contentImageType.Value.ToStr();
                    fileStorage.Lang = language;
                    FileStorageRepository.SaveOrEdit(fileStorage);
                    file.fileStorageId = fileStorage.Id;

                    var sTags = selectedTags.Split(",".ToCharArray()).Select(r => r.ToInt());
                    if (sTags.Any())
                    {
                        FileStorageTagRepository.DeleteByWhereCondition(r => r.FileStorageId == file.fileStorageId);
                        foreach (var imageTag in sTags)
                        {
                            var iTag = new FileStorageTag();
                            iTag.TagId = imageTag;
                            iTag.FileStorageId = file.fileStorageId;
                            FileStorageTagRepository.SaveOrEdit(iTag);
                        }
                    }

                    switch (contentMediaType.Value)
                    {
                        case MediaModType.Stories:
                            var sf = new StoryFile();
                            sf.StoryId = contentId;
                            sf.FileStorageId = fileStorage.Id;
                            sf.Name = fileStorage.Name;
                            sf.CreatedDate = DateTime.Now;
                            sf.UpdatedDate = DateTime.Now;
                            sf.IsActive = true;
                            sf.Position = 1;
                            sf.Lang = language;
                            StoryFileRepository.SaveOrEdit(sf);
                            break;

                        case MediaModType.Products:
                            var pf = new ProductFile();
                            pf.ProductId = contentId;
                            pf.FileStorageId = fileStorage.Id;
                            pf.Name = fileStorage.Name;
                            pf.CreatedDate = DateTime.Now;
                            pf.UpdatedDate = DateTime.Now;
                            pf.IsActive = true;
                            pf.Position = 1;
                            pf.Lang = language;
                            ProductFileRepository.SaveOrEdit(pf);
                            break;

                        case MediaModType.Menus:
                            var mf = new MenuFile();
                            mf.MenuId = contentId;
                            mf.FileStorageId = fileStorage.Id;
                            mf.Name = fileStorage.Name;
                            mf.CreatedDate = DateTime.Now;
                            mf.UpdatedDate = DateTime.Now;
                            mf.IsActive = true;
                            mf.Position = 1;
                            mf.Lang = language;
                            MenuFileRepository.SaveOrEdit(mf);
                            break;

                        default:
                            break;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                    Logger.Error(ex, Constants.DbEntityValidationExceptionPrefix + message);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "ContentId:" + contentId +
                        " contentImageType:" + contentImageType.Value
                        + " contentMediaType:" + contentMediaType.Value);
                }
            }
        }

        public async Task SaveUploadImagesAsync(int contentId,
            EImeceImageType? contentImageType,
            MediaModType? contentMediaType,
            List<ViewDataUploadFilesResult> resultList,
            int language,
            string selectedTags
            )
        {
            foreach (var file in resultList)
            {
                try
                {
                    var fileStorage = new FileStorage();
                    fileStorage.Name = file.name;
                    fileStorage.FileName = file.name;
                    fileStorage.Width = file.width;
                    fileStorage.Height = file.height;
                    fileStorage.MimeType = file.mimeType;
                    fileStorage.CreatedDate = DateTime.Now;
                    fileStorage.UpdatedDate = DateTime.Now;
                    fileStorage.IsActive = true;
                    fileStorage.Position = 1;
                    fileStorage.FileSize = file.size;
                    fileStorage.IsFileExist = PhysicalFileExists(fileStorage.FileName);
                    fileStorage.Type = contentImageType.Value.ToStr();
                    fileStorage.Lang = language;
                    await FileStorageRepository.SaveOrEditAsync(fileStorage).ConfigureAwait(false);
                    file.fileStorageId = fileStorage.Id;

                    var sTags = selectedTags.Split(",".ToCharArray()).Select(r => r.ToInt());
                    if (sTags.Any())
                    {
                        await FileStorageTagRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == file.fileStorageId).ConfigureAwait(false);
                        foreach (var imageTag in sTags)
                        {
                            var iTag = new FileStorageTag();
                            iTag.TagId = imageTag;
                            iTag.FileStorageId = file.fileStorageId;
                            await FileStorageTagRepository.SaveOrEditAsync(iTag).ConfigureAwait(false);
                        }
                    }

                    switch (contentMediaType.Value)
                    {
                        case MediaModType.Stories:
                            var sf = new StoryFile();
                            sf.StoryId = contentId;
                            sf.FileStorageId = fileStorage.Id;
                            sf.Name = fileStorage.Name;
                            sf.CreatedDate = DateTime.Now;
                            sf.UpdatedDate = DateTime.Now;
                            sf.IsActive = true;
                            sf.Position = 1;
                            sf.Lang = language;
                            await StoryFileRepository.SaveOrEditAsync(sf).ConfigureAwait(false);
                            break;

                        case MediaModType.Products:
                            var pf = new ProductFile();
                            pf.ProductId = contentId;
                            pf.FileStorageId = fileStorage.Id;
                            pf.Name = fileStorage.Name;
                            pf.CreatedDate = DateTime.Now;
                            pf.UpdatedDate = DateTime.Now;
                            pf.IsActive = true;
                            pf.Position = 1;
                            pf.Lang = language;
                            await ProductFileRepository.SaveOrEditAsync(pf).ConfigureAwait(false);
                            break;

                        case MediaModType.Menus:
                            var mf = new MenuFile();
                            mf.MenuId = contentId;
                            mf.FileStorageId = fileStorage.Id;
                            mf.Name = fileStorage.Name;
                            mf.CreatedDate = DateTime.Now;
                            mf.UpdatedDate = DateTime.Now;
                            mf.IsActive = true;
                            mf.Position = 1;
                            mf.Lang = language;
                            await MenuFileRepository.SaveOrEditAsync(mf).ConfigureAwait(false);
                            break;

                        default:
                            break;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                    Logger.Error(ex, Constants.DbEntityValidationExceptionPrefix + message);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "ContentId:" + contentId +
                        " contentImageType:" + contentImageType.Value
                        + " contentMediaType:" + contentMediaType.Value);
                }
            }
        }

        public void DeleteUploadImage(String fileName, int contentId, EImeceImageType? imageType, MediaModType? mod)
        {
            FileStorage f = FileStorageRepository.GetFileStoragebyFileName(fileName);
            if (f == null)
            {
                TryDeletePhysicalFile(fileName);
                Logger.Info("Deleted orphan media file {0} (no FileStorage row) contentId={1}", fileName, contentId);
                return;
            }

            DeleteUploadImageByFileStorage(contentId, mod, f.Id);
        }

        public void DeleteUploadImage(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod)
        {
            FileStorage f = FileStorageRepository.GetSingle(fileStorageId);
            if (f == null)
            {
                Logger.Warn("DeleteUploadImage skipped missing FileStorageId={0}", fileStorageId);
                return;
            }

            DeleteUploadImageByFileStorage(contentId, mod, f.Id);
        }

        public async Task DeleteUploadImageAsync(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod)
        {
            FileStorage f = await FileStorageRepository.GetSingleAsync(fileStorageId).ConfigureAwait(false);
            if (f == null)
            {
                Logger.Warn("DeleteUploadImageAsync skipped missing FileStorageId={0}", fileStorageId);
                return;
            }

            await DeleteUploadImageByFileStorageAsync(contentId, mod, f.Id).ConfigureAwait(false);
        }

        public void DeleteUploadImageByFileStorage(int contentId, MediaModType? mod, int fileStorageId)
        {
            if (!mod.HasValue)
            {
                DeleteFileStorage(fileStorageId);
                return;
            }

            switch (mod.Value)
            {
                case MediaModType.Stories:
                    StoryFileRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorageId && r.StoryId == contentId);
                    this.DeleteFileStorage(fileStorageId);
                    break;

                case MediaModType.Products:
                    ProductFileRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorageId && r.ProductId == contentId);
                    this.DeleteFileStorage(fileStorageId);
                    break;

                case MediaModType.Menus:
                    MenuFileRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorageId && r.MenuId == contentId);
                    this.DeleteFileStorage(fileStorageId);
                    break;

                default:
                    this.DeleteFileStorage(fileStorageId);
                    break;
            }
        }

        public async Task DeleteUploadImageByFileStorageAsync(int contentId, MediaModType? mod, int fileStorageId)
        {
            if (!mod.HasValue)
            {
                await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                return;
            }

            switch (mod.Value)
            {
                case MediaModType.Stories:
                    await StoryFileRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorageId && r.StoryId == contentId).ConfigureAwait(false);
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;

                case MediaModType.Products:
                    await ProductFileRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorageId && r.ProductId == contentId).ConfigureAwait(false);
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;

                case MediaModType.Menus:
                    await MenuFileRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorageId && r.MenuId == contentId).ConfigureAwait(false);
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;

                default:
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;
            }
        }

        public void DeleteGalleryImages(int contentId, MediaModType mod)
        {
            List<int> fileStorageIds = GetGalleryFileStorageIds(contentId, mod);
            foreach (int fileStorageId in fileStorageIds)
            {
                DeleteUploadImageByFileStorage(contentId, mod, fileStorageId);
            }
        }

        public async Task DeleteGalleryImagesAsync(int contentId, MediaModType mod)
        {
            List<int> fileStorageIds = await GetGalleryFileStorageIdsAsync(contentId, mod).ConfigureAwait(false);
            foreach (int fileStorageId in fileStorageIds)
            {
                await DeleteUploadImageByFileStorageAsync(contentId, mod, fileStorageId).ConfigureAwait(false);
            }
        }

        public int DeleteMissingFiles(int contentId, MediaModType mod, EImeceImageType imageType)
        {
            var images = GetUploadImages(contentId, mod, imageType);
            if (images == null || images.Count == 0)
            {
                return 0;
            }

            int deletedCount = 0;
            foreach (var fileStorage in images)
            {
                if (fileStorage == null) continue;

                bool exists = PhysicalFileExists(fileStorage.FileName);

                if (!exists)
                {
                    DeleteUploadImageByFileStorage(contentId, mod, fileStorage.Id);
                    deletedCount++;
                    Logger.Warn("Deleted orphan FileStorage record Id={0} FileName={1} (physical file missing on disk) for contentId={2}, mod={3}, imageType={4}",
                        fileStorage.Id, fileStorage.FileName, contentId, mod, imageType);
                }
            }

            return deletedCount;
        }

        public async Task<int> DeleteMissingFilesAsync(int contentId, MediaModType mod, EImeceImageType imageType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var images = await GetUploadImagesAsync(contentId, mod, imageType, cancellationToken).ConfigureAwait(false);
            if (images == null || images.Count == 0)
            {
                return 0;
            }

            int deletedCount = 0;
            foreach (var fileStorage in images)
            {
                if (fileStorage == null) continue;

                bool exists = PhysicalFileExists(fileStorage.FileName);

                if (!exists)
                {
                    await DeleteUploadImageByFileStorageAsync(contentId, mod, fileStorage.Id).ConfigureAwait(false);
                    deletedCount++;
                    Logger.Warn("Deleted orphan FileStorage record Id={0} FileName={1} (physical file missing on disk) for contentId={2}, mod={3}, imageType={4}",
                        fileStorage.Id, fileStorage.FileName, contentId, mod, imageType);
                }
            }

            return deletedCount;
        }

        private List<int> GetGalleryFileStorageIds(int contentId, MediaModType mod)
        {
            switch (mod)
            {
                case MediaModType.Products:
                    return ProductFileRepository.FindBy(r => r.ProductId == contentId).Select(r => r.FileStorageId).Distinct().ToList();

                case MediaModType.Stories:
                    return StoryFileRepository.FindBy(r => r.StoryId == contentId).Select(r => r.FileStorageId).Distinct().ToList();

                case MediaModType.Menus:
                    return MenuFileRepository.FindBy(r => r.MenuId == contentId).Select(r => r.FileStorageId).Distinct().ToList();

                default:
                    return new List<int>();
            }
        }

        private async Task<List<int>> GetGalleryFileStorageIdsAsync(int contentId, MediaModType mod)
        {
            switch (mod)
            {
                case MediaModType.Products:
                    return await ProductFileRepository.FindBy(r => r.ProductId == contentId).Select(r => r.FileStorageId).Distinct().ToListAsync().ConfigureAwait(false);

                case MediaModType.Stories:
                    return await StoryFileRepository.FindBy(r => r.StoryId == contentId).Select(r => r.FileStorageId).Distinct().ToListAsync().ConfigureAwait(false);

                case MediaModType.Menus:
                    return await MenuFileRepository.FindBy(r => r.MenuId == contentId).Select(r => r.FileStorageId).Distinct().ToListAsync().ConfigureAwait(false);

                default:
                    return new List<int>();
            }
        }

        public List<FileStorage> GetUploadImages(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType)
        {
            var typeStr = enumImageType.ToStr();
            switch (enumMod.Value)
            {
                case MediaModType.Stories:
                    Expression<Func<StoryFile, object>> includeProperty = r => r.FileStorage;
                    Expression<Func<StoryFile, object>>[] includeProperties = { includeProperty };
                    Expression<Func<StoryFile, bool>> match = r => r.StoryId == contentId && r.FileStorage.Type.Equals(typeStr, StringComparison.InvariantCultureIgnoreCase);

                    var item = StoryFileRepository.FindAllIncluding(match, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties).ToList();
                    return item.Select(r => r.FileStorage).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Products:
                    Expression<Func<ProductFile, object>> includeProperty1 = r => r.FileStorage;
                    Expression<Func<ProductFile, object>>[] includeProperties1 = { includeProperty1 };
                    Expression<Func<ProductFile, bool>> match1 = r => r.ProductId == contentId && r.FileStorage.Type.Equals(typeStr, StringComparison.InvariantCultureIgnoreCase);

                    var item1 = ProductFileRepository.FindAllIncluding(match1, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties1).ToList();
                    return item1.Select(r => r.FileStorage).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Menus:
                    Expression<Func<MenuFile, object>> includeProperty2 = r => r.FileStorage;
                    Expression<Func<MenuFile, object>>[] includeProperties2 = { includeProperty2 };
                    Expression<Func<MenuFile, bool>> match2 = r => r.MenuId == contentId && r.FileStorage.Type.Equals(typeStr, StringComparison.InvariantCultureIgnoreCase);

                    var item2 = MenuFileRepository.FindAllIncluding(match2, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties2).ToList();
                    return item2.Select(r => r.FileStorage).OrderByDescending(r => r.UpdatedDate).ToList();

                default:
                    break;
            }

            return null;
        }

        public async Task<List<FileStorage>> GetUploadImagesAsync(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var typeStr = enumImageType.ToStr();
            switch (enumMod.Value)
            {
                case MediaModType.Stories:
                    Expression<Func<StoryFile, object>> includeProperty = r => r.FileStorage;
                    Expression<Func<StoryFile, object>>[] includeProperties = { includeProperty };
                    Expression<Func<StoryFile, bool>> match = r => r.StoryId == contentId && r.FileStorage.Type.Equals(typeStr, StringComparison.InvariantCultureIgnoreCase);

                    var item = await StoryFileRepository.FindAllIncluding(match, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return item.Select(r => r.FileStorage).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Products:
                    Expression<Func<ProductFile, object>> includeProperty1 = r => r.FileStorage;
                    Expression<Func<ProductFile, object>>[] includeProperties1 = { includeProperty1 };
                    Expression<Func<ProductFile, bool>> match1 = r => r.ProductId == contentId && r.FileStorage.Type.Equals(typeStr, StringComparison.InvariantCultureIgnoreCase);

                    var item1 = await ProductFileRepository.FindAllIncluding(match1, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties1).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return item1.Select(r => r.FileStorage).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Menus:
                    Expression<Func<MenuFile, object>> includeProperty2 = r => r.FileStorage;
                    Expression<Func<MenuFile, object>>[] includeProperties2 = { includeProperty2 };
                    Expression<Func<MenuFile, bool>> match2 = r => r.MenuId == contentId && r.FileStorage.Type.Equals(typeStr, StringComparison.InvariantCultureIgnoreCase);

                    var item2 = await MenuFileRepository.FindAllIncluding(match2, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties2).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return item2.Select(r => r.FileStorage).OrderByDescending(r => r.UpdatedDate).ToList();

                default:
                    break;
            }

            return null;
        }

        public override void DeleteBaseEntity(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            try
            {
                foreach (String v in values)
                {
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        continue;
                    }

                    // Expected format: fileStorageId-contentId-mod[-imageType]
                    var parts = v.Split('-');
                    if (parts.Length < 3)
                    {
                        Logger.Error("DeleteBaseEntity skipped invalid media key '" + v + "'. Expected fileStorageId-contentId-mod[-imageType].");
                        continue;
                    }

                    var fileStorageId = parts[0].ToInt();
                    int contentId = parts[1].ToInt();
                    MediaModType? enumMod = EnumHelper.Parse<MediaModType>(parts[2].ToStr());
                    if (!enumMod.HasValue || fileStorageId <= 0 || contentId <= 0)
                    {
                        Logger.Error("DeleteBaseEntity skipped unparseable media key '" + v + "'.");
                        continue;
                    }

                    DeleteUploadImageByFileStorage(contentId, enumMod, fileStorageId);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.Error(ex, Constants.DbEntityValidationExceptionPrefix + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public override async Task DeleteBaseEntityAsync(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            try
            {
                foreach (String v in values)
                {
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        continue;
                    }

                    var parts = v.Split('-');
                    if (parts.Length < 3)
                    {
                        Logger.Error("DeleteBaseEntity skipped invalid media key '" + v + "'. Expected fileStorageId-contentId-mod[-imageType].");
                        continue;
                    }

                    var fileStorageId = parts[0].ToInt();
                    int contentId = parts[1].ToInt();
                    MediaModType? enumMod = EnumHelper.Parse<MediaModType>(parts[2].ToStr());
                    if (!enumMod.HasValue || fileStorageId <= 0 || contentId <= 0)
                    {
                        Logger.Error("DeleteBaseEntity skipped unparseable media key '" + v + "'.");
                        continue;
                    }

                    await DeleteUploadImageByFileStorageAsync(contentId, enumMod, fileStorageId).ConfigureAwait(false);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.Error(ex, Constants.DbEntityValidationExceptionPrefix + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public string DeleteFileStorage(int id)
        {
            try
            {
                var fileStorage = GetSingle(id);
                if (fileStorage != null)
                {
                    FileStorageTagRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorage.Id);

                    string fileName = fileStorage.FileName;
                    if (!FilesHelper.IsSeedPlaceholderMedia(fileStorage)
                        && !string.Equals(fileName, FilesHelper.EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
                    {
                        TryDeletePhysicalFile(fileName);
                    }

                    DeleteEntity(fileStorage);
                    Logger.Info("Deleted FileStorage Id={0} FileName={1}", id, fileName);
                    return "Ok";
                }

                return Constants.ErrorResult;
            }
            catch (Exception exception)
            {
                var innerExpMessage = exception.InnerException == null ? "" : exception.InnerException.Message;
                Logger.Error(exception, exception.Message + " - DeleteFileStorage Id :" + id + "" + innerExpMessage);
            }
            return Constants.ErrorResult;
        }

        public async Task<string> DeleteFileStorageAsync(int id)
        {
            try
            {
                var fileStorage = await GetSingleAsync(id).ConfigureAwait(false);
                if (fileStorage != null)
                {
                    await FileStorageTagRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorage.Id).ConfigureAwait(false);

                    string fileName = fileStorage.FileName;
                    if (!FilesHelper.IsSeedPlaceholderMedia(fileStorage)
                        && !string.Equals(fileName, FilesHelper.EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
                    {
                        TryDeletePhysicalFile(fileName);
                    }

                    await DeleteEntityAsync(fileStorage).ConfigureAwait(false);
                    Logger.Info("Deleted FileStorage Id={0} FileName={1}", id, fileName);
                    return "Ok";
                }

                return Constants.ErrorResult;
            }
            catch (Exception exception)
            {
                var innerExpMessage = exception.InnerException == null ? "" : exception.InnerException.Message;
                Logger.Error(exception, exception.Message + " - DeleteFileStorage Id :" + id + "" + innerExpMessage);
            }
            return Constants.ErrorResult;
        }
    }
}