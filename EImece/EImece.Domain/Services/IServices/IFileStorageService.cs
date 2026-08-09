using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IFileStorageService : IBaseEntityService<FileStorage>
    {
        void SaveUploadImages(int contentId,
            EImeceImageType? contentImageType,
            MediaModType? contentMediaType,
            List<ViewDataUploadFilesResult> resultList,
            int language, string selectedTags);

        void DeleteUploadImage(String fileName, int contentId, EImeceImageType? imageType, MediaModType? mod);

        List<FileStorage> GetUploadImages(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType);

        string DeleteFileStorage(int id);

        FileStorage GetFileStorage(int fileStorageId);

        Task<FileStorage> GetFileStorageAsync(int fileStorageId, CancellationToken cancellationToken = default(CancellationToken));

        void DeleteUploadImage(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod);

        void DeleteUploadImageByFileStorage(int contentId, MediaModType? mod, int fileStorageId);
    }
}