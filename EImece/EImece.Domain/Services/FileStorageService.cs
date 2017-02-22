using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using Ninject;
using EImece.Domain.Helpers;
using NLog;
using System.Linq.Expressions;
using GenericRepository.EntityFramework.Enums;
using System.Data.Entity.Validation;

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

        public void SaveUploadImages(int contentId, EImeceImageType? contentImageType, MediaModType? contentMediaType, List<ViewDataUploadFilesResult> resultList)
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
                    fileStorage.Type = contentImageType.Value.ToStr();
                    FileStorageRepository.SaveOrEdit(fileStorage);
                    file.fileStorageId = fileStorage.Id;

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

        public void DeleteUploadImage(String fileName, int contentId, EImeceImageType? imageType, MediaModType? mod)
        {
            FileStorage f = FileStorageRepository.GetFileStoragebyFileName(fileName);
            bool isResult = false;
            switch (mod.Value)
            {
                case MediaModType.Stories:
                    isResult = StoryFileRepository.DeleteByWhereCondition(r => r.FileStorageId == f.Id && r.StoryId == contentId);
                    FileStorageRepository.DeleteItem(f);
                    break;

                case MediaModType.Products:
                    isResult = ProductFileRepository.DeleteByWhereCondition(r => r.FileStorageId == f.Id && r.ProductId == contentId);
                    FileStorageRepository.DeleteItem(f);
                    break;
                case MediaModType.Menus:
                    isResult = MenuFileRepository.DeleteByWhereCondition(r => r.FileStorageId == f.Id && r.MenuId == contentId);
                    FileStorageRepository.DeleteItem(f);
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

                    var item = StoryFileRepository.FindAllIncluding(match,  r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties).ToList();
                    return item.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r=>r.UpdatedDate).ToList();

                case MediaModType.Products:
                    Expression<Func<ProductFile, object>> includeProperty1 = r => r.FileStorage;
                    Expression<Func<ProductFile, object>>[] includeProperties1 = { includeProperty1 };
                    Expression<Func<ProductFile, bool>> match1 = r => r.ProductId == contentId;

                    var item1 = ProductFileRepository.FindAllIncluding(match1,  r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties1).ToList();
                    return item1.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                case MediaModType.Menus:
                    Expression<Func<MenuFile, object>> includeProperty2 = r => r.FileStorage;
                    Expression<Func<MenuFile, object>>[] includeProperties2 = { includeProperty2 };
                    Expression<Func<MenuFile, bool>> match2 = r => r.MenuId == contentId;

                    var item2 = MenuFileRepository.FindAllIncluding(match2,  r => r.FileStorageId, OrderByType.Ascending, null, null, includeProperties2).ToList();
                    return item2.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.UpdatedDate).ToList();

                default:
                    break;
            }

            return null;
        }

        public override void DeleteBaseEntity(List<string> values)
        {
            try
            {
                var deletedResult = "";

                foreach (String v in values)
                {
                    var parts = v.Split("-".ToCharArray());
                    var fileStorageId = parts[0].ToInt();
                    int contentId = parts[1].ToInt();
                    MediaModType? enumMod = EnumHelper.Parse<MediaModType>(parts[2].ToStr());
                    EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(parts[3].ToStr());
                    var fileStorage = FileStorageRepository.GetSingle(fileStorageId);
                    deletedResult = FilesHelper.DeleteFile(fileStorage.FileName);
                    switch (enumMod.Value)
                    {
                        case MediaModType.Stories:
                            if (deletedResult.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                            {
                                StoryFileRepository.DeleteByWhereCondition(r => r.StoryId == contentId && r.FileStorageId == fileStorageId);
                                FileStorageRepository.Delete(fileStorage);
                            }
                            break;
                        case MediaModType.Products:
                            if (deletedResult.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ProductFileRepository.DeleteByWhereCondition(r => r.ProductId == contentId && r.FileStorageId == fileStorageId);
                                FileStorageRepository.Delete(fileStorage);
                            }
                            break;
                        case MediaModType.Menus:
                            if (deletedResult.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                            {
                                MenuFileRepository.DeleteByWhereCondition(r => r.MenuId == contentId && r.FileStorageId == fileStorageId);
                                FileStorageRepository.Delete(fileStorage);
                            }
                            break;

                        default:
                            break;
                    }
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
                Logger.Error(exception, exception.Message + " - DeleteFileStorage Id :" + id);
            }
            return "error";
        }
    }
}
