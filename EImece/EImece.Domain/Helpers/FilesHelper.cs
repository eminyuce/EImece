using EImece.Domain.Entities;
using EImece.Domain.Models.AdminHelperModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace EImece.Domain.Helpers
{
    public class FilesHelper : IDisposable
    {
        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        private const string THUMBS = "thumbs";
        private const string THB = "thb";
        public const string EXTERNAL_IMAGE = "external-image";
        private const string MEDIA_FILE_LOCATION = "/media/images/";
        private const string MEDIA_FILE_LOCATION_THUMBS = "/media/images/thumbs/";
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public int CurrentLanguage { get; set; }

        public string DeleteURL { get; set; }
        public string DeleteType { get; set; }
        public string StorageRoot { get; set; }
        public string UrlBase { get; set; }
        public string TempPath { get; set; }
        public string ServerMapPath { get; set; }

        public void InitFilesMediaFolder()
        {
            Init(Constants.DeleteURL, Constants.DeleteType, AppConfig.StorageRoot, Constants.UrlBase, Constants.TempPath, Constants.ServerMapPath);
        }

        public void InitFilesMediaFolder(String deleteUrl)
        {
            Init(deleteUrl, Constants.DeleteType, AppConfig.StorageRoot, Constants.UrlBase, Constants.TempPath, Constants.ServerMapPath);
        }

        private void Init(string deleteURL, string deleteType, string storageRoot, string urlBase,
            string tempPath, string serverMapPath)
        {
            this.DeleteURL = deleteURL;
            this.DeleteType = deleteType;
            this.StorageRoot = storageRoot;
            this.UrlBase = urlBase;
            this.TempPath = tempPath;
            this.ServerMapPath = serverMapPath;
        }

        public SavedImage GetThumbnailImageSize(int mainPageId)
        {
            var mainPage = FileStorageService.GetFileStorage(mainPageId);
            return GetThumbnailImageSize(mainPage);
        }

        public SavedImage GetThumbnailImageSize(FileStorage mainImage)
        {
            int thumpBitmapWidth = 0, thumpBitmapHeight = 0;
            int originalWidth = 0, originalHeight = 0;
            String fileName = "";
            if (mainImage != null)
            {
                fileName = mainImage.FileName;
                return GetThumbnailImageSize(fileName);
            }
            var result = new SavedImage(thumpBitmapWidth, thumpBitmapHeight, originalWidth, originalHeight, fileName);
            return result;
        }

        public SavedImage GetThumbnailImageSize(String fileName)
        {
            int thumpBitmapWidth = 0, thumpBitmapHeight = 0;
            int originalWidth = 0, originalHeight = 0;
            String fullPath = SecurityHelper.GetSafeStorageFilePath(StorageRoot, fileName);
            String thumbPath = "/thb" + Path.GetFileName(fileName) + "";
            String partThumb1 = Path.Combine(StorageRoot, THUMBS);
            String partThumb2 = Path.Combine(partThumb1, THB + Path.GetFileName(fileName));
            if (File.Exists(partThumb2))
            {
                using (Bitmap thumpBitmap = new Bitmap(partThumb2))
                {
                    thumpBitmapWidth = thumpBitmap.Width;
                    thumpBitmapHeight = thumpBitmap.Height;
                    thumpBitmap.Dispose();

                    if (File.Exists(partThumb2))
                    {
                        Bitmap fullBitmap = new Bitmap(fullPath);
                        originalWidth = fullBitmap.Width;
                        originalHeight = fullBitmap.Height;
                        fullBitmap.Dispose();
                    }
                }
            }

            var result = new SavedImage(thumpBitmapWidth, thumpBitmapHeight, originalWidth, originalHeight, fileName);
            return result;
        }

        public void DeleteFiles(String pathToDelete)
        {
            string path = HostingEnvironment.MapPath(pathToDelete);

            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    File.Delete(fi.FullName);
                }

                di.Delete(true);
            }
        }

        public String DeleteFile(String file, HttpContextBase ContentBase)
        {
            var request = ContentBase.Request;
            int contentId = request.QueryString["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.QueryString["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.QueryString["mod"].ToStr());

            FileStorageService.DeleteUploadImage(file, contentId, imageType, mod);

            return "OK";
        }

        public string DeleteThumbFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return "Ok";
            }

            EnsureStorageInitialized();
            string safeFileName = Path.GetFileName(file);
            if (string.IsNullOrEmpty(safeFileName) || safeFileName.Equals(EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return "Ok";
            }

            TryDeletePhysicalFile(GetThumbnailPhysicalPath(safeFileName));
            return "Ok";
        }

        public bool NormalFileExists(String file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return false;
            }

            EnsureStorageInitialized();
            String fullPath = SecurityHelper.GetSafeStorageFilePath(StorageRoot, file);
            return File.Exists(fullPath);
        }

        public String DeleteNormalFile(String file)
        {
            if (string.IsNullOrWhiteSpace(file) || file.Equals(EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return "Ok";
            }

            EnsureStorageInitialized();
            TryDeleteSidecarWebP(file);
            string fullPath = SecurityHelper.GetSafeStorageFilePath(StorageRoot, file);
            TryDeletePhysicalFile(fullPath);
            return "Ok";
        }

        private void TryDeleteSidecarWebP(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return;
            }

            string ext = Path.GetExtension(file);
            if (string.Equals(ext, ".webp", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                string webpName = Path.GetFileName(Path.ChangeExtension(file, ".webp"));
                if (string.IsNullOrEmpty(webpName))
                {
                    return;
                }

                string webpPath = SecurityHelper.GetSafeStorageFilePath(StorageRoot, webpName);
                TryDeletePhysicalFile(webpPath);
                TryDeletePhysicalFile(GetThumbnailPhysicalPath(webpName));
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Could not delete WebP sidecar for {0}", file);
            }
        }

        public String DeleteFile(String file)
        {
            if (string.IsNullOrWhiteSpace(file) || file.Equals(EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return "Ok";
            }

            EnsureStorageInitialized();
            DeleteStoredImageFiles(StorageRoot, file);
            return "Ok";
        }

        /// <summary>
        /// Removes the stored image, its thumb, and optional WebP sidecars. Missing files are treated as success.
        /// </summary>
        public static void DeleteStoredImageFiles(string storageRoot, string fileName)
        {
            if (string.IsNullOrWhiteSpace(storageRoot) || string.IsNullOrWhiteSpace(fileName)
                || fileName.Equals(EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName))
            {
                return;
            }

            TryDeletePhysicalFile(Path.Combine(storageRoot, safeName));
            TryDeletePhysicalFile(Path.Combine(storageRoot, THUMBS, THB + safeName));

            string ext = Path.GetExtension(safeName);
            if (!string.Equals(ext, ".webp", StringComparison.OrdinalIgnoreCase))
            {
                string webpName = Path.ChangeExtension(safeName, ".webp");
                TryDeletePhysicalFile(Path.Combine(storageRoot, webpName));
                TryDeletePhysicalFile(Path.Combine(storageRoot, THUMBS, THB + webpName));
            }
        }

        private void EnsureStorageInitialized()
        {
            if (string.IsNullOrWhiteSpace(StorageRoot))
            {
                InitFilesMediaFolder();
            }
        }

        private static bool TryDeletePhysicalFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return true;
            }

            try
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    string pending = path + ".deleted";
                    if (File.Exists(pending))
                    {
                        File.SetAttributes(pending, FileAttributes.Normal);
                        File.Delete(pending);
                    }

                    File.Move(path, pending);
                    File.Delete(pending);
                    return true;
                }
                catch (Exception retryEx)
                {
                    Logger.Error(retryEx, "Failed to delete image file {0} (first error: {1})", path, ex.Message);
                    return false;
                }
            }
        }

        public JsonFiles GetFileList(HttpContextBase ContentBase)
        {
            var request = ContentBase.Request;
            int Id = request.QueryString["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.QueryString["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.QueryString["mod"].ToStr());

            var r = new List<ViewDataUploadFilesResult>();

            String fullPath = Path.Combine(StorageRoot);
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo dir = new DirectoryInfo(fullPath);
                foreach (FileInfo file in dir.GetFiles())
                {
                    int SizeInt = unchecked((int)file.Length);
                    r.Add(UploadResult(file.Name, SizeInt, file.FullName, ContentBase));
                }
            }
            JsonFiles files = new JsonFiles(r);

            return files;
        }

        public void UploadAndShowResults(HttpContextBase ContentBase, List<ViewDataUploadFilesResult> resultList)
        {
            if (ContentBase?.Request == null || resultList == null)
            {
                return;
            }

            EnsureStorageInitialized();
            UploadWholeFile(ContentBase, resultList);
        }

        private void UploadWholeFile(HttpContextBase requestContext, List<ViewDataUploadFilesResult> statuses)
        {
            var request = requestContext.Request;
            int height = request.Form["imageHeight"].ToInt();
            int width = request.Form["imageWidth"].ToInt();

            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var ext = Path.GetExtension(file.FileName);
                if (IsImage(ext))
                {
                    var result = SaveImageByte(width, height, file);
                    var newFileName = result.NewFileName;
                    var k = UploadResult(newFileName, result.ImageSize, newFileName, requestContext);
                    k.imageHash = result.FileHash;
                    statuses.Add(k);
                }
            }
        }

        public ViewDataUploadFilesResult UploadResult(String FileName, int fileSize, String FileFullPath, HttpContextBase requestContext)
        {
            var request = requestContext.Request;
            int contentId = request.Form["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.Form["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.Form["mod"].ToStr());

            String getType = MimeMapping.GetMimeMapping(FileFullPath);
            if (string.Equals(Path.GetExtension(FileName), ".webp", StringComparison.OrdinalIgnoreCase))
            {
                getType = ImageUploadOptimizer.MimeWebP;
            }

            String patchOnServer = Path.Combine(StorageRoot);
            var fullName = Path.Combine(patchOnServer, Path.GetFileName(FileName));
            int storedWidth = 0;
            int storedHeight = 0;
            try
            {
                using (Bitmap img = LoadImage(fullName))
                {
                    storedWidth = img.Width;
                    storedHeight = img.Height;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Could not read stored image dimensions for upload JSON: {0}", fullName);
            }

            var result = new ViewDataUploadFilesResult()
            {
                name = FileName,
                size = fileSize,
                type = getType,
                width = storedWidth,
                height = storedHeight,
                mimeType = getType,
                url = UrlBase + FileName,
                deleteUrl = String.Format(DeleteURL, FileName, contentId, mod, imageType),
                thumbnailUrl = CheckThumb(getType, FileName),
                deleteType = DeleteType,
            };
            return result;
        }

        public String CheckThumb(String type, String FileName)
        {
            var splited = type.Split('/');
            if (splited.Length == 2)
            {
                string extansion = splited[1];
                if (extansion.Equals("jpeg") || extansion.Equals("jpg") || extansion.Equals("png") || extansion.Equals("gif") || extansion.Equals("webp"))
                {
                    //   String thumbnailUrl = UrlBase + "/thumbs/" + FileName + ".80x80.jpg";
                    String thumbnailUrl = UrlBase + "/thumbs/thb" + FileName;
                    return thumbnailUrl;
                }
                else
                {
                    if (extansion.Equals("octet-stream")) //Fix for exe files
                    {
                        return "/Content/Free-file-icons/48px/exe.png";
                    }
                    if (extansion.Contains("zip")) //Fix for exe files
                    {
                        return "/Content/Free-file-icons/48px/zip.png";
                    }
                    String thumbnailUrl = "/Content/Free-file-icons/48px/" + extansion + ".png";
                    return thumbnailUrl;
                }
            }
            else
            {
                return UrlBase + "/thumbs/" + FileName + ".80x80.jpg";
            }
        }

        public List<String> FilesList()
        {
            List<String> Filess = new List<String>();
            string path = HostingEnvironment.MapPath(ServerMapPath);
            System.Diagnostics.Debug.WriteLine(path);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    Filess.Add(fi.Name);
                }
            }
            return Filess;
        }

        public static bool IsMainImageExists(int? MainImageId, FileStorage MainImage)
        {
            if (MainImageId.HasValue && MainImage != null && MainImage.FileName.Equals(EXTERNAL_IMAGE))
            {
                return !string.IsNullOrEmpty(MainImage.FileUrl);
            }
            else if (MainImageId.HasValue && MainImage != null)
            {
                String fullPath = Path.Combine(AppConfig.StorageRoot, MainImage.FileName);
                return File.Exists(fullPath);
            }
            return false;
        }

        public FileStorage SaveFileFromByteArray(byte[] imageByte, String fileName, String contentType,
            int height = 0,
            int width = 0,
            EImeceImageType imageType = EImeceImageType.NONE, int? mainImageId = null)
        {
            if (mainImageId.HasValue && mainImageId.Value > 0)
            {
                FileStorageService.DeleteFileStorage(mainImageId.Value);
            }
            var result = SaveImageByte(width, height, fileName, contentType, imageByte);

            FileStorage fileStorage = createFileStorageFromSavedImage(imageType, result);
            fileStorage.IsFileExist = NormalFileExists(fileStorage.FileName);
            FileStorageService.SaveOrEditEntity(fileStorage);
            return fileStorage;
        }

        public void SaveFileFromHttpPostedFileBase(HttpPostedFileBase httpPostedFileBase,
            int height = 0,
            int width = 0,
            EImeceImageType imageType = EImeceImageType.NONE,
            BaseContent baseContent = null)
        {
            if (httpPostedFileBase != null)
            {
                if (baseContent.MainImageId.HasValue)
                {
                    String deleted = FileStorageService.DeleteFileStorage(baseContent.MainImageId.Value);
                }
                SavedImage result = SaveImageByte(width, height, httpPostedFileBase);
                FileStorage fileStorage = createFileStorageFromSavedImage(imageType, result);
                FileStorageService.SaveOrEditEntity(fileStorage);
                baseContent.MainImageId = fileStorage.Id;
                baseContent.ImageState = true;
            }
            else if (baseContent.MainImageId.HasValue)
            {
                var mainImage = FileStorageService.GetFileStorage(baseContent.MainImageId.Value);
                if (mainImage != null)
                {
                    var imageSize = GetThumbnailImageSize(mainImage);
                    int mainImageHeight = imageSize.ThumpBitmapHeight;
                    int mainImageWidth = imageSize.ThumpBitmapWidth;
                    if (mainImageHeight != height && mainImageWidth != width) //Resize thumb image with new dimension.
                    {
                        String fullPath = Path.Combine(StorageRoot, mainImage.FileName);
                        String candidatePathThb = Path.Combine(Path.Combine(StorageRoot, THUMBS), THB + mainImage.FileName);
                        try
                        {
                            var fileByte = File.ReadAllBytes(fullPath);
                            int origW = imageSize.OriginalWidth;
                            int origH = imageSize.OriginalHeight;
                            if (origW <= 0 || origH <= 0)
                            {
                                using (Bitmap src = ByteArrayToBitmap(fileByte))
                                {
                                    origW = src.Width;
                                    origH = src.Height;
                                }
                            }

                            Size thumbSize = ResolveThumbnailTargetSize(width, height, origW, origH);
                            string forceExt = Path.GetExtension(mainImage.FileName);
                            var thumbOpt = ImageUploadOptimizer.Optimize(fileByte, ImageUploadOptimizeOptions.ForThumbnail(
                                mainImage.FileName, mainImage.MimeType, thumbSize.Width, thumbSize.Height, forceExt));

                            DeleteThumbFile(mainImage.FileName);
                            SaveBytesToFilePath(thumbOpt.Bytes, candidatePathThb);
                            baseContent.ImageState = true;
                            Logger.Info("Regenerated compressed thumb for {0}: {1} bytes ({2}x{3})",
                                mainImage.FileName, thumbOpt.Bytes.Length, thumbOpt.Width, thumbOpt.Height);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Thumb regeneration failed for {0}", mainImage.FileName);
                        }
                    }
                }
            }
        }

        private FileStorage createFileStorageFromSavedImage(EImeceImageType imageType, SavedImage result)
        {
            var fileStorage = new FileStorage();
            fileStorage.Name = result.FileName;
            fileStorage.FileName = result.NewFileName;
            fileStorage.Width = result.Width;
            fileStorage.Height = result.Height;
            fileStorage.MimeType = result.ContentType;
            fileStorage.CreatedDate = DateTime.Now;
            fileStorage.UpdatedDate = DateTime.Now;
            fileStorage.IsActive = true;
            fileStorage.Position = 1;
            fileStorage.FileSize = result.ImageSize;
            fileStorage.Type = imageType.ToStr();
            fileStorage.Lang = CurrentLanguage;
            fileStorage.IsFileExist = NormalFileExists(fileStorage.FileName);
            return fileStorage;
        }

        private Tuple<string, string, string> GetFileNames(String fileName)
        {
            var ext = Path.GetExtension(fileName);
            var fileBase = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_");
            Random random = new Random();
            var randomNumber = random.Next(0, int.MaxValue).ToString();
            var newFileName = string.Format(@"{0}_{1}{2}", fileBase, randomNumber, ext);

            String fullPath = Path.Combine(StorageRoot, newFileName);
            String partThumb1 = Path.Combine(StorageRoot, THUMBS);
            String candidatePathThb = Path.Combine(partThumb1, THB + newFileName);

            return new Tuple<string, string, string>(fullPath, candidatePathThb, newFileName);
        }

        public Tuple<string, string, string> GetFileNames2(String fileName)
        {
            String fullPath = Path.Combine(StorageRoot, fileName);
            String partThumb1 = Path.Combine(StorageRoot, THUMBS);
            String candidatePathThb = Path.Combine(partThumb1, THB + fileName);

            return new Tuple<string, string, string>(fullPath, candidatePathThb, fileName);
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private static void SaveImageToFilePath(Image img, string filePath, ImageFormat format)
        {
            EnsureDirectoryExists(filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Intentionally ignored: deleting a locked existing file is best-effort; the file is overwritten next.
                    Logger.Debug(ex, "Could not delete existing file before save: {0}", filePath);
                }
            }

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                img.Save(fs, format);
            }
        }

        public SavedImage SaveImageByte(int width, int height, String fileName, String contentType, byte[] fileByte)
        {
            String fullPath = "", candidatePathThb = "", newFileName = "";
            int imageSize = 0;
            String fileHash = "";

            fileName = Path.GetFileName(fileName);
            var ext = Path.GetExtension(fileName).ToLower();

            if (IsImage(ext))
            {
                if (fileByte == null || fileByte.Length == 0)
                {
                    throw new ArgumentException("File byte array cannot be null or empty.", nameof(fileByte));
                }

                ImageOptimizationResult fullOpt = OptimizeAndSaveImage(fileByte, fileName, contentType);
                string storedName = Path.ChangeExtension(fileName, fullOpt.Extension);
                var fileNames = GetFileNames(storedName);
                fullPath = fileNames.Item1;
                candidatePathThb = fileNames.Item2;
                newFileName = fileNames.Item3;

                SaveBytesToFilePath(fullOpt.Bytes, fullPath);

                Size thumbTarget = ResolveThumbnailTargetSize(width, height, fullOpt.OriginalWidth, fullOpt.OriginalHeight);
                thumbTarget = ImageUploadOptimizer.FitWithin(thumbTarget.Width, thumbTarget.Height, fullOpt.Width, fullOpt.Height);
                var thumbOpt = ImageUploadOptimizer.Optimize(
                    fileByte,
                    ImageUploadOptimizeOptions.ForThumbnail(fileName, contentType, thumbTarget.Width, thumbTarget.Height, fullOpt.Extension));
                SaveBytesToFilePath(thumbOpt.Bytes, candidatePathThb);

                if (AppConfig.ImageUploadSaveWebPSidecar && !fullOpt.IsWebP)
                {
                    try
                    {
                        saveWebPformat(fullPath, fullOpt.Bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "WebP sidecar save failed for {0}", fullPath);
                    }
                }

                width = fullOpt.Width;
                height = fullOpt.Height;
                imageSize = fullOpt.Bytes.Length;
                contentType = fullOpt.MimeType;
                fileHash = HashHelpers.GetSha256Hash(fullOpt.Bytes);

                Logger.Info(
                    "Image upload optimized. file={0} originalBytes={1} originalSize={2}x{3} storedBytes={4} storedSize={5}x{6} mime={7} keptOriginal={8} thumbBytes={9} thumbSize={10}x{11}",
                    newFileName,
                    fullOpt.OriginalSize,
                    fullOpt.OriginalWidth,
                    fullOpt.OriginalHeight,
                    fullOpt.Bytes.Length,
                    fullOpt.Width,
                    fullOpt.Height,
                    fullOpt.MimeType,
                    fullOpt.KeptOriginal,
                    thumbOpt.Bytes.Length,
                    thumbOpt.Width,
                    thumbOpt.Height);
            }
            else
            {
                fileHash = "Image Extension is not CORRECT:" + fileName;
                Logger.Error("Image Extension is not CORRECT:" + fileName);
            }

            return new SavedImage(newFileName, width, height, imageSize, contentType, fileName, fileHash);
        }

        /// <summary>
        /// Resize/compress source bytes for primary storage. Public signatures of SaveImageByte are unchanged.
        /// </summary>
        internal ImageOptimizationResult OptimizeAndSaveImage(byte[] source, string fileName, string contentType)
        {
            try
            {
                return ImageUploadOptimizer.Optimize(source, ImageUploadOptimizeOptions.ForFullImage(fileName, contentType));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Image optimization failed for {0}; storing a best-effort re-encode.", fileName);
                var fallback = new ImageUploadOptimizeOptions
                {
                    MaxWidth = AppConfig.ImageUploadMaxWidth,
                    MaxHeight = AppConfig.ImageUploadMaxHeight,
                    JpegQuality = AppConfig.ImageUploadJpegQuality,
                    WebPQuality = AppConfig.ImageUploadWebPQuality,
                    PreferWebP = false,
                    KeepOriginalIfSmaller = true,
                    SourceExtension = Path.GetExtension(fileName),
                    SourceMimeType = contentType
                };
                return ImageUploadOptimizer.Optimize(source, fallback);
            }
        }

        private static Size ResolveThumbnailTargetSize(int requestedWidth, int requestedHeight, int originalWidth, int originalHeight)
        {
            Size fromRequest;
            if (requestedWidth > 0 || requestedHeight > 0)
            {
                int maxW = requestedWidth > 0 ? requestedWidth : int.MaxValue;
                int maxH = requestedHeight > 0 ? requestedHeight : int.MaxValue;
                fromRequest = ImageUploadOptimizer.FitWithin(originalWidth, originalHeight, maxW, maxH);
            }
            else
            {
                fromRequest = ImageUploadOptimizer.FitWithin(
                    originalWidth,
                    originalHeight,
                    AppConfig.ImageUploadThumbMaxWidth,
                    AppConfig.ImageUploadThumbMaxHeight);
            }

            return ImageUploadOptimizer.FitWithin(
                fromRequest.Width,
                fromRequest.Height,
                AppConfig.ImageUploadThumbMaxWidth,
                AppConfig.ImageUploadThumbMaxHeight);
        }

        private static void SaveBytesToFilePath(byte[] bytes, string filePath)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new InvalidOperationException("Cannot save empty image bytes to " + filePath);
            }

            EnsureDirectoryExists(filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Could not delete existing file before save: {0}", filePath);
                }
            }

            File.WriteAllBytes(filePath, bytes);
        }

        private void saveWebPformat(string fullPath, byte[] byteArrayIn)
        {
            string webPFileName = Path.GetFileNameWithoutExtension(fullPath) + ".webp";
            string webPImagePath = Path.Combine(StorageRoot, webPFileName);
            EnsureDirectoryExists(webPImagePath);
            using (FileStream webPFileStream = new FileStream(webPImagePath, FileMode.Create))
            {
                ISupportedImageFormat lg_format = new WebPFormat { Quality = AppConfig.ImageUploadWebPQuality };
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(byteArrayIn)
                                .Format(lg_format)
                                .Save(webPFileStream);
                }
            }
        }

        public SavedImage SaveImageByte(int width, int height, HttpPostedFileBase file)
        {
            var fileByte = GeneralHelper.ReadFully(file.InputStream);
            return SaveImageByte(width, height, file.FileName, file.ContentType, fileByte);
        }

        public ImageFormat GetImageFormat(String extension)
        {
            extension = extension.Replace(".", "");
            switch (extension)
            {
                case "jpeg": return ImageFormat.Jpeg;
                case "jpg": return ImageFormat.Jpeg;
                case "png": return ImageFormat.Png;
                case "icon": return ImageFormat.Icon;
                case "gif": return ImageFormat.Gif;
                case "bmp": return ImageFormat.Bmp;
                case "tiff": return ImageFormat.Tiff;
                case "emf": return ImageFormat.Emf;
                case "wmf": return ImageFormat.Wmf;
            }

            return ImageFormat.Jpeg;
        }

        public SavedImage GetResizedImage(int fileStorageId, int width, int height)
        {
            SavedImage result = null;
            FileStorage fileStorage;
            byte[] imageBytes = GetFileStorageFromCache(fileStorageId, out fileStorage);
            if (imageBytes == null)
            {
                return null;
            }

            result = resizeImageBytesByWidthAndHeight(imageBytes, width, height, fileStorage.MimeType);
            result.UpdatedDated = fileStorage.UpdatedDate;
            return result;
        }

        public async Task<SavedImage> GetResizedImageAsync(int fileStorageId, int width, int height, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loaded = await GetFileStorageFromCacheAsync(fileStorageId, cancellationToken).ConfigureAwait(false);
            if (loaded.Item1 == null || loaded.Item2 == null)
            {
                return null;
            }

            var result = resizeImageBytesByWidthAndHeight(loaded.Item1, width, height, loaded.Item2.MimeType);
            result.UpdatedDated = loaded.Item2.UpdatedDate;
            return result;
        }

        public async Task<SavedImage> GetResizedImageAsWebPAsync(int fileStorageId, int width, int height, int quality = 80, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loaded = await GetFileStorageFromCacheAsync(fileStorageId, cancellationToken).ConfigureAwait(false);
            if (loaded.Item1 == null || loaded.Item2 == null)
            {
                return null;
            }

            try
            {
                using (var startStream = new MemoryStream(loaded.Item1))
                using (var bitmap = new Bitmap(startStream))
                using (var resized = ResizeImage(bitmap, width, height))
                using (var outStream = new MemoryStream())
                {
                    ISupportedImageFormat webPFormat = new WebPFormat { Quality = quality };
                    using (var imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        using (var loadStream = new MemoryStream(GetBitmapBytes(resized)))
                        {
                            imageFactory.Load(loadStream)
                                .Format(webPFormat)
                                .Save(outStream);
                        }
                    }

                    var result = new SavedImage(outStream.ToArray(), "image/webp");
                    result.UpdatedDated = loaded.Item2.UpdatedDate;
                    result.Width = resized.Width;
                    result.Height = resized.Height;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "WebP conversion failed for fileStorageId={0}; falling back to original mime type", fileStorageId);
                return await GetResizedImageAsync(fileStorageId, width, height, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Resize and encode as WebP when the client advertises image/webp support.
        /// Falls back to the standard resized image on failure.
        /// </summary>
        public SavedImage GetResizedImageAsWebP(int fileStorageId, int width, int height, int quality = 80)
        {
            FileStorage fileStorage;
            byte[] imageBytes = GetFileStorageFromCache(fileStorageId, out fileStorage);
            if (imageBytes == null)
            {
                return null;
            }

            try
            {
                using (var startStream = new MemoryStream(imageBytes))
                using (var bitmap = new Bitmap(startStream))
                using (var resized = ResizeImage(bitmap, width, height))
                using (var outStream = new MemoryStream())
                {
                    ISupportedImageFormat webPFormat = new WebPFormat { Quality = quality };
                    using (var imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        using (var loadStream = new MemoryStream(GetBitmapBytes(resized)))
                        {
                            imageFactory.Load(loadStream)
                                .Format(webPFormat)
                                .Save(outStream);
                        }
                    }

                    var result = new SavedImage(outStream.ToArray(), "image/webp");
                    result.UpdatedDated = fileStorage.UpdatedDate;
                    result.Width = resized.Width;
                    result.Height = resized.Height;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "WebP conversion failed for fileStorageId={0}; falling back to original mime type", fileStorageId);
                return GetResizedImage(fileStorageId, width, height);
            }
        }

        public Tuple<string, string> GetImageSrcPath(int fileStorageId)
        {
            var fileStorage = FileStorageService.GetFileStorage(fileStorageId);
            return GetFileStorageSrcPath(fileStorage);
        }

        public static Tuple<string, string> GetFileStorageSrcPath(FileStorage fileStorage)
        {
            if (fileStorage == null)
            {
                return new Tuple<string, string>("", "");
            }

            // Seed demo JPEGs paint "EImece Media {filename}" into the pixels — never expose
            // those as static /media/images URLs. Callers fall through to the resize proxy /
            // abstract placeholder instead.
            if (IsSeedPlaceholderMedia(fileStorage))
            {
                return new Tuple<string, string>("", "");
            }

            return GetFileStorageSrcPath(fileStorage.FileName);
        }

        /// <summary>
        /// SeedDummyData marks demo FileStorage rows with FileUrl under /media/seed/.
        /// GenerateSeedImages historically burned the filename into those JPEGs.
        /// </summary>
        public static bool IsSeedPlaceholderMedia(FileStorage fileStorage)
        {
            if (fileStorage == null)
            {
                return false;
            }

            var url = fileStorage.FileUrl ?? string.Empty;
            return url.IndexOf("/media/seed/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static Tuple<string, string> GetFileStorageSrcPath(String fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                String fullPath = Path.Combine(AppConfig.StorageRoot, fileName);
                if (File.Exists(fullPath))
                {
                    var fullPathImgSrc = MEDIA_FILE_LOCATION + fileName;
                    var candidatePathThb = MEDIA_FILE_LOCATION_THUMBS + fileName;
                    return new Tuple<string, string>(fullPathImgSrc, candidatePathThb);
                }
            }
            return new Tuple<string, string>("", "");
        }

        /// <summary>
        /// Physical path for the prebuilt upload thumbnail (media/images/thumbs/thb{fileName}).
        /// </summary>
        public static string GetThumbnailPhysicalPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Equals(EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName))
            {
                return null;
            }

            return Path.Combine(AppConfig.StorageRoot, THUMBS, THB + safeName);
        }

        public static bool ThumbnailFileExists(string fileName)
        {
            var path = GetThumbnailPhysicalPath(fileName);
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        /// <summary>
        /// Public URL for the prebuilt thumbnail, e.g. /media/images/thumbs/thbfoo.jpg
        /// </summary>
        public static string GetThumbnailPublicUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Equals(EXTERNAL_IMAGE, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName))
            {
                return null;
            }

            return MEDIA_FILE_LOCATION_THUMBS + THB + safeName;
        }

        /// <summary>
        /// True when the requested display size can be served by downscaling the prebuilt thumb
        /// (or when stored thumb dims are unknown but the request is modest).
        /// </summary>
        public static bool CanServeRequestFromThumbnail(int requestWidth, int requestHeight, int thumbWidth, int thumbHeight)
        {
            if (requestWidth <= 0 && requestHeight <= 0)
            {
                return false;
            }

            if (thumbWidth > 0 && thumbHeight > 0)
            {
                bool widthOk = requestWidth <= 0 || requestWidth <= thumbWidth;
                bool heightOk = requestHeight <= 0 || requestHeight <= thumbHeight;
                return widthOk && heightOk;
            }

            // Unknown thumb metadata: only use thumb for small UI slots.
            const int unknownThumbMaxSide = 400;
            return Math.Max(requestWidth, requestHeight) <= unknownThumbMaxSide;
        }

        public byte[] GetFileStorageFromCache(int fileStorageId, out FileStorage fileStorage)
        {
            byte[] imageBytes = null;
            fileStorage = FileStorageService.GetFileStorage(fileStorageId);
            if (fileStorage != null)
            {
                if (IsSeedPlaceholderMedia(fileStorage))
                {
                    int w = fileStorage.Width > 0 ? fileStorage.Width : 1200;
                    int h = fileStorage.Height > 0 ? fileStorage.Height : 900;
                    imageBytes = GenerateAbstractPlaceholder(fileStorage.Id, w, h);
                }
                else
                {
                    String fullPath = Path.Combine(StorageRoot, fileStorage.FileName);
                    if (File.Exists(fullPath))
                    {
                        imageBytes = File.ReadAllBytes(fullPath);
                    }
                }
            }
            return imageBytes;
        }

        /// <summary>
        /// Async twin of <see cref="GetFileStorageFromCache"/>. Item1 is the file bytes; Item2 is the
        /// FileStorage row. Disk read stays synchronous after the metadata await (local media folder).
        /// </summary>
        public async Task<Tuple<byte[], FileStorage>> GetFileStorageFromCacheAsync(int fileStorageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileStorage = await FileStorageService.GetFileStorageAsync(fileStorageId, cancellationToken).ConfigureAwait(false);
            if (fileStorage == null)
            {
                return Tuple.Create<byte[], FileStorage>(null, null);
            }

            if (IsSeedPlaceholderMedia(fileStorage))
            {
                int w = fileStorage.Width > 0 ? fileStorage.Width : 1200;
                int h = fileStorage.Height > 0 ? fileStorage.Height : 900;
                return Tuple.Create(GenerateAbstractPlaceholder(fileStorage.Id, w, h), fileStorage);
            }

            String fullPath = Path.Combine(StorageRoot, fileStorage.FileName);
            if (!File.Exists(fullPath))
            {
                return Tuple.Create<byte[], FileStorage>(null, fileStorage);
            }

            byte[] imageBytes = File.ReadAllBytes(fullPath);
            return Tuple.Create(imageBytes, fileStorage);
        }

        private SavedImage resizeImageBytesByWidthAndHeight(byte[] imageBytes, int width, int height, string mimeType)
        {
            // Stop.
            SavedImage result = null;
            using (MemoryStream StartMemoryStream = new MemoryStream(), NewMemoryStream = new System.IO.MemoryStream())
            {
                // write the string to the stream
                StartMemoryStream.Write(imageBytes, 0, imageBytes.Length);

                // create the start Bitmap from the MemoryStream that contains the image
                Bitmap startBitmap = new Bitmap(StartMemoryStream);
                var resizeBitmap = ResizeImage(startBitmap, width, height);
                byte[] resizedImageBytes = GetBitmapBytes(resizeBitmap);
                result = new SavedImage(resizedImageBytes, mimeType);
                startBitmap.Dispose();
                resizeBitmap.Dispose();
            }

            return result;
        }

        // Create a thumbnail in byte array format from the image encoded in the passed byte array.
        // (RESIZE an image in a byte[] variable.)
        public byte[] CreateThumbnail(byte[] PassedImage, int LargestSide, int Height, int Width, ImageFormat format)
        {
            byte[] ReturnedThumbnail;

            using (MemoryStream StartMemoryStream = new MemoryStream(), NewMemoryStream = new System.IO.MemoryStream())
            {
                // write the string to the stream
                StartMemoryStream.Write(PassedImage, 0, PassedImage.Length);

                // create the start Bitmap from the MemoryStream that contains the image
                Bitmap startBitmap = new Bitmap(StartMemoryStream);

                // set thumbnail height and width proportional to the original image.
                int newHeight;
                int newWidth;
                double HW_ratio;
                if (startBitmap.Height > startBitmap.Width)
                {
                    newHeight = LargestSide;
                    HW_ratio = ((double)LargestSide / (double)startBitmap.Height);
                    newWidth = (int)(HW_ratio * startBitmap.Width);
                }
                else
                {
                    newWidth = LargestSide;
                    HW_ratio = ((double)LargestSide / (double)startBitmap.Width);
                    newHeight = (int)(HW_ratio * (double)startBitmap.Height);
                }
                newHeight = Height;
                newWidth = Width;
                // create a new Bitmap with dimensions for the thumbnail.
                System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(newWidth, newHeight);

                // Copy the image from the START Bitmap into the NEW Bitmap.
                // This will create a thumnail size of the same image.
                newBitmap = ResizeImage(startBitmap, newWidth, newHeight);

                // Save this image to the specified stream in the specified format.
                newBitmap.Save(NewMemoryStream, format);

                // Fill the byte[] for the thumbnail from the new MemoryStream.
                ReturnedThumbnail = NewMemoryStream.ToArray();
                startBitmap.Dispose();
                newBitmap.Dispose();
            }
            // return the resized image as a string of bytes.
            return ReturnedThumbnail;
        }

        public void CreateThumbnail(Bitmap startBitmap, int width, int height, string imageFullPath, ImageFormat format)
        {
            using (Bitmap newBitmap = ResizeImage(startBitmap, width, height))  // Ensure proper disposal
            {
                ConvertAndSaveBitmap(newBitmap, imageFullPath, format, 100);
            }
        }

        public byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        public byte[] BitmapToByteArray(Bitmap imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public Bitmap ByteArrayToBitmap(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Bitmap returnImage = new Bitmap(ms);
            return returnImage;
        }

        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            double ratio = (double)image.Height / (double)image.Width;
            if (width > 0 && height == 0)
            {
                height = (int)Math.Round(width * ratio);
            }
            else if (width == 0 && height > 0)
            {
                width = (int)Math.Round(height / ratio);
            }

            if (width > 0 && height > 0)
            {
                Bitmap resizedImage = new Bitmap(width, height);
                using (Graphics gfx = Graphics.FromImage(resizedImage))
                {
                    gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                        new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                }
                return resizedImage;
            }
            return image;
        }

        public static Bitmap ConvertAndSaveBitmap(Bitmap bitmap, String fileName, ImageFormat imageFormat, long quality = 100L)
        {
            string contentType = HttpContext.Current.Response.ContentType;
            var extension = Path.GetExtension(fileName);
            using (var encoderParameters = new EncoderParameters(1))
            using (encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality))
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
                HttpContext.Current.Response.ContentType = codecs[1].MimeType;
                bitmap.Save(fileName, GetImageCodecInfo(extension), encoderParameters);
            }
            HttpContext.Current.Response.ContentType = contentType;

            return bitmap;
        }

        public static bool IsImage(string ext)
        {
            ext = ext.ToLower();
            return ext == ".gif" || ext == ".jpg" || ext == ".png" || ext == ".bmp" || ext == ".tiff" || ext == ".jpe" || ext == ".jpeg" || ext == ".webp";
        }

        /// <summary>
        /// Determines if a file is a known image type by checking the extension.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file is an image.</returns>
        public static bool IsImageByFileName(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return IsImage(ext);
        }

        private string EncodeFile(string fileName)
        {
            return System.Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public static byte[] CropImage(byte[] content, int x, int y, int width, int height)
        {
            using (MemoryStream stream = new MemoryStream(content))
            {
                return CropImage(stream, x, y, width, height);
            }
        }

        public static byte[] CropImage(Stream content, int x, int y, int width, int height)
        {
            //Parsing stream to bitmap
            using (Bitmap sourceBitmap = new Bitmap(content))
            {
                //Get new dimensions
                double sourceWidth = Convert.ToDouble(sourceBitmap.Size.Width);
                double sourceHeight = Convert.ToDouble(sourceBitmap.Size.Height);
                Rectangle cropRect = new Rectangle(x, y, width, height);

                //Creating new bitmap with valid dimensions
                using (Bitmap newBitMap = new Bitmap(cropRect.Width, cropRect.Height))
                {
                    using (Graphics g = Graphics.FromImage(newBitMap))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        g.DrawImage(sourceBitmap, new Rectangle(0, 0, newBitMap.Width, newBitMap.Height), cropRect, GraphicsUnit.Pixel);

                        return GetBitmapBytes(newBitMap);
                    }
                }
            }
        }

        public static byte[] GetBitmapBytes(Bitmap source)
        {
            //Settings to increase quality of the image
            ImageCodecInfo codec = GetEncoderInfo(source.RawFormat) ?? GetEncoderInfo(ImageFormat.Png);
            using (EncoderParameters parameters = new EncoderParameters(1))
            {
                parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

                //Temporary stream to save the bitmap
                using (MemoryStream tmpStream = new MemoryStream())
                {
                    source.Save(tmpStream, codec, parameters);
                    return tmpStream.ToArray();
                }
            }
        }

        private static ImageCodecInfo GetImageCodecInfo(string extension)
        {
            switch (extension)
            {
                case ".bmp": return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Bmp.Guid);
                case ".jpg": return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                case ".jpeg": return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                case ".gif": return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Gif.Guid);
                case ".tiff": return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Tiff.Guid);
                case ".png": return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Png.Guid);
                default: return null;
            }
        }

        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>
        public void Save(Bitmap image, int maxWidth, int maxHeight, int quality, string filePath, ImageFormat format)
        {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            EnsureDirectoryExists(filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Intentionally ignored: deleting a locked existing file is best-effort; the resized file is overwritten next.
                    Logger.Debug(ex, "Could not delete existing file before resize save: {0}", filePath);
                }
            }

            // Get an ImageCodecInfo object that represents the format codec.
            ImageCodecInfo imageCodecInfo = GetEncoderInfo(format) ?? GetEncoderInfo(ImageFormat.Jpeg);

            // Create an Encoder object for the Quality parameter.
            var encoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            using (EncoderParameters encoderParameters = new EncoderParameters(1))
            {
                EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                encoderParameters.Param[0] = encoderParameter;
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    newImage.Save(fs, imageCodecInfo, encoderParameters);
                }
            }
        }

        /// <summary>
        /// Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            if (format == null) return ImageCodecInfo.GetImageEncoders().FirstOrDefault();
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == format.Guid)
                   ?? ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
        }

        public static Bitmap LoadImage(string path)
        {
            //  var ms = new MemoryStream(File.ReadAllBytes(path));
            //  GC.KeepAlive(ms);
            // return (Bitmap)Image.FromStream(ms);
            // Use File.ReadAllBytes to load bytes, then create a bitmap from them
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                return new Bitmap(ms);
            }
        }

        public static Size GetThumbnailSize(Image original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 40;

            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

        public Byte[] GenerateDefaultImg(string text = "", int width = 200, int height = 200)
        {
            // Ignore legacy text overlays (e.g. "X") — never dump filenames into placeholders.
            return GenerateAbstractPlaceholder(0, width > 0 ? width : 200, height > 0 ? height : 200);
        }

        /// <summary>
        /// Soft branded abstract JPEG with no filename / label text (safe for hero + PDP).
        /// </summary>
        public static byte[] GenerateAbstractPlaceholder(int seedKey, int width, int height)
        {
            if (width <= 0) width = 800;
            if (height <= 0) height = 600;
            if (width > 2400) width = 2400;
            if (height > 2400) height = 2400;

            var palette = new[]
            {
                Color.FromArgb(255, 20, 33, 43),
                Color.FromArgb(255, 9, 184, 80),
                Color.FromArgb(255, 26, 51, 64),
                Color.FromArgb(255, 10, 125, 58),
                Color.FromArgb(255, 15, 26, 34),
                Color.FromArgb(255, 14, 163, 74)
            };
            var baseColor = palette[Math.Abs(seedKey) % palette.Length];
            var accent = palette[(Math.Abs(seedKey) + 1) % palette.Length];

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(width, height))
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                using (var bg = new LinearGradientBrush(
                    new Rectangle(0, 0, width, height),
                    baseColor,
                    accent,
                    35f))
                {
                    gfx.FillRectangle(bg, 0, 0, width, height);
                }

                using (var veil = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
                {
                    gfx.FillEllipse(veil, (int)(width * 0.45), (int)(-height * 0.15), (int)(width * 0.7), (int)(height * 0.7));
                }

                using (var veil2 = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    gfx.FillEllipse(veil2, (int)(-width * 0.2), (int)(height * 0.35), (int)(width * 0.65), (int)(height * 0.8));
                }

                bmp.Save(mem, ImageFormat.Jpeg);
                return mem.ToArray();
            }
        }

        public Byte[] GenerateCaptchaImg(string captcha = "", bool includenoise = true)
        {
            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                if (includenoise)
                {
                    int i, r, x, y;
                    using (var pen = new Pen(Color.Yellow))
                    {
                        Random rand = new Random();

                        for (i = 1; i < 10; i++)
                        {
                            pen.Color = Color.FromArgb(
                            (rand.Next(0, 255)),
                            (rand.Next(0, 255)),
                            (rand.Next(0, 255)));

                            r = rand.Next(0, (130 / 3));
                            x = rand.Next(0, 130);
                            y = rand.Next(0, 30);

                            gfx.DrawEllipse(pen, x - r, y - r, r, r);
                        }
                    }
                }

                using (var captchaFont = new Font("Tahoma", 15))
                {
                    gfx.DrawString(captcha, captchaFont, Brushes.Black, 2, 3);
                }

                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                return mem.GetBuffer();
            }
        }

        // Existing fields and properties

        private bool _disposed = false;
        private List<IDisposable> _disposableResources = new List<IDisposable>();

        // Track resources created by this class that need disposal
        private void TrackResource(IDisposable resource)
        {
            if (resource != null)
            {
                _disposableResources.Add(resource);
            }
        }

        // Implement IDisposable pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    foreach (var resource in _disposableResources)
                    {
                        resource?.Dispose();
                    }
                    _disposableResources.Clear();
                }

                // No unmanaged resources to dispose directly

                _disposed = true;
            }
        }

        // Add finalizer
        ~FilesHelper()
        {
            Dispose(false);
        }
    }

    public class JsonFiles
    {
        public ViewDataUploadFilesResult[] files;
        public string TempFolder { get; set; }

        public JsonFiles(List<ViewDataUploadFilesResult> filesList)
        {
            files = new ViewDataUploadFilesResult[filesList.Count];
            for (int i = 0; i < filesList.Count; i++)
            {
                files[i] = filesList.ElementAt(i);
            }
        }
    }

    public enum AnchorPosition
    {
        Top,
        Center,
        Bottom,
        Left,
        Right
    }
}