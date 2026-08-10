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

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        public IFileStorageRepository FileStorageRepository { get; set; }

        public FileStorageService(IFileStorageRepository repository) : base(repository)
        {
            FileStorageRepository = repository;
        }

        public FileStorage GetFileStorage(int fileStorageId)
        {
            var result = GetFileStorages().FirstOrDefault(r => r.Id == fileStorageId);
            if (result == null)
            {
                result = GetSingle(fileStorageId);
            }
            return result;
        }

        public async Task<FileStorage> GetFileStorageAsync(int fileStorageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = (await GetFileStoragesAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(r => r.Id == fileStorageId);
            if (result == null)
            {
                result = await GetSingleAsync(fileStorageId).ConfigureAwait(false);
            }
            return result;
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
                    //fileStorage.EntityHash = file.imageHash;
                    fileStorage.IsFileExist = FilesHelper.NormalFileExists(fileStorage.FileName);
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
                    Logger.Error(ex, "DbEntityValidationException:" + message);
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
                    //fileStorage.EntityHash = file.imageHash;
                    fileStorage.IsFileExist = FilesHelper.NormalFileExists(fileStorage.FileName);
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
                    Logger.Error(ex, "DbEntityValidationException:" + message);
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
            DeleteUploadImageByFileStorage(contentId, mod, f.Id);
        }

        public void DeleteUploadImage(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod)
        {
            FileStorage f = FileStorageRepository.GetSingle(fileStorageId);
            DeleteUploadImageByFileStorage(contentId, mod, f.Id);
        }

        public async Task DeleteUploadImageAsync(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod)
        {
            FileStorage f = await FileStorageRepository.GetSingleAsync(fileStorageId).ConfigureAwait(false);
            await DeleteUploadImageByFileStorageAsync(contentId, mod, f.Id).ConfigureAwait(false);
        }

        public void DeleteUploadImageByFileStorage(int contentId, MediaModType? mod, int fileStorageId)
        {
            bool isResult = false;
            switch (mod.Value)
            {
                case MediaModType.Stories:
                    isResult = StoryFileRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorageId && r.StoryId == contentId);
                    this.DeleteFileStorage(fileStorageId);
                    break;

                case MediaModType.Products:
                    isResult = ProductFileRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorageId && r.ProductId == contentId);
                    this.DeleteFileStorage(fileStorageId);
                    break;

                case MediaModType.Menus:
                    isResult = MenuFileRepository.DeleteByWhereCondition(r => r.FileStorageId == fileStorageId && r.MenuId == contentId);
                    this.DeleteFileStorage(fileStorageId);
                    break;

                default:
                    break;
            }
        }

        public async Task DeleteUploadImageByFileStorageAsync(int contentId, MediaModType? mod, int fileStorageId)
        {
            bool isResult = false;
            switch (mod.Value)
            {
                case MediaModType.Stories:
                    isResult = await StoryFileRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorageId && r.StoryId == contentId).ConfigureAwait(false);
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;

                case MediaModType.Products:
                    isResult = await ProductFileRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorageId && r.ProductId == contentId).ConfigureAwait(false);
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;

                case MediaModType.Menus:
                    isResult = await MenuFileRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorageId && r.MenuId == contentId).ConfigureAwait(false);
                    await this.DeleteFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    break;

                default:
                    break;
            }
        }

        public List<FileStorage> GetUploadImages(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType)
        {
            switch (enumMod.Value)
            {
                case MediaModType.Stories:
                    Expression<Func<StoryFile, object>> includeProperty = r => r.FileStorage;
                    Expression<Func<StoryFile, object>>[] includeProperties = { includeProperty };
                    Expression<Func<StoryFile, bool>> match = r => r.StoryId == contentId;

                    var item = StoryFileRepository.FindAllIncluding(match, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties).ToList();
                    return item.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Products:
                    Expression<Func<ProductFile, object>> includeProperty1 = r => r.FileStorage;
                    Expression<Func<ProductFile, object>>[] includeProperties1 = { includeProperty1 };
                    Expression<Func<ProductFile, bool>> match1 = r => r.ProductId == contentId;

                    var item1 = ProductFileRepository.FindAllIncluding(match1, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties1).ToList();
                    return item1.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Menus:
                    Expression<Func<MenuFile, object>> includeProperty2 = r => r.FileStorage;
                    Expression<Func<MenuFile, object>>[] includeProperties2 = { includeProperty2 };
                    Expression<Func<MenuFile, bool>> match2 = r => r.MenuId == contentId;

                    var item2 = MenuFileRepository.FindAllIncluding(match2, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties2).ToList();
                    return item2.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                default:
                    break;
            }

            return null;
        }

        public async Task<List<FileStorage>> GetUploadImagesAsync(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (enumMod.Value)
            {
                case MediaModType.Stories:
                    Expression<Func<StoryFile, object>> includeProperty = r => r.FileStorage;
                    Expression<Func<StoryFile, object>>[] includeProperties = { includeProperty };
                    Expression<Func<StoryFile, bool>> match = r => r.StoryId == contentId;

                    var item = await StoryFileRepository.FindAllIncluding(match, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return item.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Products:
                    Expression<Func<ProductFile, object>> includeProperty1 = r => r.FileStorage;
                    Expression<Func<ProductFile, object>>[] includeProperties1 = { includeProperty1 };
                    Expression<Func<ProductFile, bool>> match1 = r => r.ProductId == contentId;

                    var item1 = await ProductFileRepository.FindAllIncluding(match1, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties1).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return item1.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Menus:
                    Expression<Func<MenuFile, object>> includeProperty2 = r => r.FileStorage;
                    Expression<Func<MenuFile, object>>[] includeProperties2 = { includeProperty2 };
                    Expression<Func<MenuFile, bool>> match2 = r => r.MenuId == contentId;

                    var item2 = await MenuFileRepository.FindAllIncluding(match2, r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties2).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return item2.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

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
                Logger.Error(ex, "DbEntityValidationException:" + message);
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
                Logger.Error(ex, "DbEntityValidationException:" + message);
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

                    var deletedResult = FilesHelper.DeleteFile(fileStorage.FileName);
                    DeleteEntity(fileStorage);
                    return deletedResult;
                }
                else
                {
                    return "error";
                }
            }
            catch (Exception exception)
            {
                var innerExpMessage = exception.InnerException == null ? "" : exception.InnerException.Message;
                Logger.Error(exception, exception.Message + " - DeleteFileStorage Id :" + id + "" + innerExpMessage);
            }
            return "error";
        }

        public async Task<string> DeleteFileStorageAsync(int id)
        {
            try
            {
                var fileStorage = await GetSingleAsync(id).ConfigureAwait(false);
                if (fileStorage != null)
                {
                    await FileStorageTagRepository.DeleteByWhereConditionAsync(r => r.FileStorageId == fileStorage.Id).ConfigureAwait(false);

                    var deletedResult = FilesHelper.DeleteFile(fileStorage.FileName);
                    await DeleteEntityAsync(fileStorage).ConfigureAwait(false);
                    return deletedResult;
                }
                else
                {
                    return "error";
                }
            }
            catch (Exception exception)
            {
                var innerExpMessage = exception.InnerException == null ? "" : exception.InnerException.Message;
                Logger.Error(exception, exception.Message + " - DeleteFileStorage Id :" + id + "" + innerExpMessage);
            }
            return "error";
        }
    }
}