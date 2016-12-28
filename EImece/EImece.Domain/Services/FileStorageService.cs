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
            switch (mod.Value)
            {
                case MediaModType.Stories:
                    StoryFileRepository.DeleteEntityByWhere(r => r.FileStorageId == f.Id && r.StoryId == contentId);
                    FileStorageRepository.DeleteItem(f);
                    break;

                case MediaModType.Products:
                    ProductFileRepository.DeleteEntityByWhere(r=>r.FileStorageId == f.Id && r.ProductId == contentId);
                    FileStorageRepository.DeleteItem(f);
                    break;
                default:
                    break;
            }
        }
    }
}
