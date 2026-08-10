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

        Task SaveUploadImagesAsync(int contentId,
            EImeceImageType? contentImageType,
            MediaModType? contentMediaType,
            List<ViewDataUploadFilesResult> resultList,
            int language, string selectedTags);

        void DeleteUploadImage(String fileName, int contentId, EImeceImageType? imageType, MediaModType? mod);

        List<FileStorage> GetUploadImages(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType);

        Task<List<FileStorage>> GetUploadImagesAsync(int contentId, MediaModType? enumMod, EImeceImageType? enumImageType, CancellationToken cancellationToken = default(CancellationToken));

        string DeleteFileStorage(int id);

        Task<string> DeleteFileStorageAsync(int id);

        FileStorage GetFileStorage(int fileStorageId);

        Task<FileStorage> GetFileStorageAsync(int fileStorageId, CancellationToken cancellationToken = default(CancellationToken));

        void DeleteUploadImage(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod);

        Task DeleteUploadImageAsync(int fileStorageId, int contentId, EImeceImageType? imageType, MediaModType? mod);

        void DeleteUploadImageByFileStorage(int contentId, MediaModType? mod, int fileStorageId);

        Task DeleteUploadImageByFileStorageAsync(int contentId, MediaModType? mod, int fileStorageId);
    }
}