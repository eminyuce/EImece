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
                    fileStorage.Width = 1;
                    fileStorage.Height = 1;
                    fileStorage.MimeType = "";
                    fileStorage.CreatedDate = DateTime.Now;
                    fileStorage.UpdatedDate = DateTime.Now;
                    fileStorage.IsActive = true;
                    fileStorage.Position = 1;
                    fileStorage.FileSize = file.size;
                    fileStorage.Type = contentImageType.Value.ToStr();
                    FileStorageRepository.SaveOrEdit(fileStorage);
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

                        default:
                            break;
                    }
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
                default:
                    break;
            }
        }

        public List<FileStorage> GetUploadImages(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType)
        {
            bool isResult = false;
            switch (enumMod.Value)
            {
                case MediaModType.Stories:
                    Expression<Func<StoryFile, object>> includeProperty = r => r.FileStorage;
                    Expression<Func<StoryFile, object>>[] includeProperties = { includeProperty };
                    Expression<Func<StoryFile, bool>> match = r => r.StoryId == contentId;

                    var item = StoryFileRepository.FindAllIncluding(match, null, null, r => r.FileStorageId, OrderByType.Ascending, includeProperties);
                    return item.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).ToList();

                case MediaModType.Products:
                    Expression<Func<ProductFile, object>> includeProperty1 = r => r.FileStorage;
                    Expression<Func<ProductFile, object>>[] includeProperties1 = { includeProperty1 };
                    Expression<Func<ProductFile, bool>> match1 = r => r.ProductId == contentId;

                    var item1 = ProductFileRepository.FindAllIncluding(match1, null, null, r => r.FileStorageId, OrderByType.Ascending, includeProperties1);
                    return item1.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).ToList();

                case MediaModType.Menus:
                    Expression<Func<MenuFile, object>> includeProperty2 = r => r.FileStorage;
                    Expression<Func<MenuFile, object>>[] includeProperties2 = { includeProperty2 };
                    Expression<Func<MenuFile, bool>> match2 = r => r.MenuId == contentId;

                    var item2 = MenuFileRepository.FindAllIncluding(match2, null, null, r => r.FileStorageId, OrderByType.Ascending, includeProperties2);
                    return item2.Select(r => r.FileStorage).Where(t => t.Type.Equals(enumImageType.ToStr(), StringComparison.InvariantCultureIgnoreCase)).ToList();

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
                FilesHelper.Init(Settings.DeleteURL, Settings.DeleteType, Settings.StorageRoot, Settings.UrlBase, Settings.TempPath, Settings.ServerMapPath);

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
                var message = GetDbEntityValidationExceptionDetail(ex);
                Logger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
    }
}
