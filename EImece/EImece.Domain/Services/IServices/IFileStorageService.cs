using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;

namespace EImece.Domain.Services.IServices
{

    public interface IFileStorageService : IBaseEntityService<FileStorage>
    {
        void SaveUploadImages(int contentId, 
            EImeceImageType? contentImageType,
            MediaModType? contentMediaType,
            List<ViewDataUploadFilesResult> resultList, 
            int language, string selectedTags);
        void DeleteUploadImage(String fileName,int contentId, EImeceImageType? imageType, MediaModType? mod);
        List<FileStorage> GetUploadImages(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType);

        string DeleteFileStorage(int id);
        FileStorage GetFileStorage(int fileStorageId);
    }
}
