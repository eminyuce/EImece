using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminHelperModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;

namespace EImece.Web.Helpers
{
    public static class WebFilesHelper
    {
        public static byte[] ToByteArray(this HttpPostedFileBase value)
        {
            if (value == null) return null;
            var array = new byte[value.ContentLength];
            value.InputStream.Position = 0;
            value.InputStream.Read(array, 0, value.ContentLength);
            return array;
        }

        public static SavedImage SaveImageByte(this FilesHelper filesHelper, int width, int height, HttpPostedFileBase file)
        {
            if (file == null) return null;
            var fileByte = GeneralHelper.ReadFully(file.InputStream);
            return filesHelper.SaveImageByte(width, height, file.FileName, file.ContentType, fileByte);
        }

        public static void SaveFileFromHttpPostedFileBase(this FilesHelper filesHelper, HttpPostedFileBase httpPostedFileBase,
            int height = 0,
            int width = 0,
            EImeceImageType imageType = EImeceImageType.NONE,
            BaseContent baseContent = null)
        {
            if (filesHelper == null || baseContent == null) return;

            if (httpPostedFileBase != null)
            {
                if (baseContent.MainImageId.HasValue)
                {
                    filesHelper.FileStorageService?.DeleteFileStorage(baseContent.MainImageId.Value);
                }
                SavedImage result = filesHelper.SaveImageByte(width, height, httpPostedFileBase);
                FileStorage fileStorage = filesHelper.createFileStorageFromSavedImage(imageType, result);
                filesHelper.FileStorageService?.SaveOrEditEntity(fileStorage);
                baseContent.MainImageId = fileStorage.Id;
                baseContent.ImageState = true;
            }
            else if (baseContent.MainImageId.HasValue)
            {
                var mainImage = filesHelper.FileStorageService?.GetFileStorage(baseContent.MainImageId.Value);
                if (mainImage != null)
                {
                    var imageSize = filesHelper.GetThumbnailImageSize(mainImage);
                    if (imageSize.Width != width || imageSize.Height != height)
                    {
                        var ext = Path.GetExtension(mainImage.FileName);
                        if (FilesHelper.IsImage(ext))
                        {
                            var fullPath = Path.Combine(filesHelper.StorageRoot, mainImage.FileName);
                            if (File.Exists(fullPath))
                            {
                                byte[] fileByte = File.ReadAllBytes(fullPath);
                                filesHelper.SaveImageByte(width, height, mainImage.FileName, mainImage.MimeType, fileByte);
                            }
                        }
                    }
                }
            }
        }

        public static void UploadAndShowResults(this FilesHelper filesHelper, HttpContextBase contentBase, List<ViewDataUploadFilesResult> resultList)
        {
            if (filesHelper == null || contentBase?.Request == null || resultList == null) return;
            filesHelper.InitFilesMediaFolder();

            var request = contentBase.Request;
            int height = request.Form["imageHeight"].ToInt();
            int width = request.Form["imageWidth"].ToInt();

            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var ext = Path.GetExtension(file.FileName);
                if (FilesHelper.IsImage(ext))
                {
                    var result = filesHelper.SaveImageByte(width, height, file);
                    var newFileName = result.NewFileName;
                    var k = filesHelper.UploadResult(newFileName, result.ImageSize, newFileName, contentBase);
                    k.imageHash = result.FileHash;
                    resultList.Add(k);
                }
            }
        }

        public static ViewDataUploadFilesResult UploadResult(this FilesHelper filesHelper, string fileName, int fileSize, string fileFullPath, HttpContextBase requestContext)
        {
            var request = requestContext.Request;
            int contentId = request.Form["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.Form["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.Form["mod"].ToStr());

            string getType = MimeMapping.GetMimeMapping(fileFullPath);
            if (string.Equals(Path.GetExtension(fileName), ".webp", StringComparison.OrdinalIgnoreCase))
            {
                getType = ImageUploadOptimizer.MimeWebP;
            }

            string patchOnServer = Path.Combine(filesHelper.StorageRoot);
            var fullName = Path.Combine(patchOnServer, Path.GetFileName(fileName));
            int storedWidth = 0;
            int storedHeight = 0;
            try
            {
                using (Bitmap img = FilesHelper.LoadImage(fullName))
                {
                    storedWidth = img.Width;
                    storedHeight = img.Height;
                }
            }
            catch
            {
            }

            return new ViewDataUploadFilesResult
            {
                name = fileName,
                size = fileSize,
                type = getType,
                width = storedWidth,
                height = storedHeight,
                mimeType = getType,
                url = filesHelper.UrlBase + fileName,
                deleteUrl = string.Format(filesHelper.DeleteURL, fileName, contentId, mod, imageType),
                thumbnailUrl = filesHelper.CheckThumb(getType, fileName),
                deleteType = filesHelper.DeleteType,
            };
        }

        public static JsonFiles GetFileList(this FilesHelper filesHelper, HttpContextBase contentBase)
        {
            var r = new List<ViewDataUploadFilesResult>();
            string fullPath = Path.Combine(filesHelper.StorageRoot);
            if (Directory.Exists(fullPath))
            {
                var dir = new DirectoryInfo(fullPath);
                foreach (FileInfo file in dir.GetFiles())
                {
                    int sizeInt = unchecked((int)file.Length);
                    r.Add(filesHelper.UploadResult(file.Name, sizeInt, file.FullName, contentBase));
                }
            }
            return new JsonFiles(r);
        }

        public static string DeleteFile(this FilesHelper filesHelper, string file, HttpContextBase contentBase)
        {
            var request = contentBase.Request;
            int contentId = request.QueryString["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.QueryString["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.QueryString["mod"].ToStr());

            filesHelper.FileStorageService?.DeleteUploadImage(file, contentId, imageType, mod);
            return "OK";
        }
    }
}
