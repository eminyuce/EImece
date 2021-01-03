using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminHelperModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace EImece.Domain.Helpers
{
    public class FilesHelper
    {
        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        private const string THUMBS = "thumbs";
        private const string THB = "thb";
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private ICacheProvider _memoryCacheProvider { get; set; }

        [Inject]
        public ICacheProvider MemoryCacheProvider
        {
            get
            {
                _memoryCacheProvider.CacheDuration = AppConfig.GetConfigInt("CacheLongSeconds");
                return _memoryCacheProvider;
            }
            set
            {
                _memoryCacheProvider = value;
            }
        }

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
            String fullPath = Path.Combine(StorageRoot, fileName);
            String thumbPath = "/thb" + fileName + "";
            String partThumb1 = Path.Combine(StorageRoot, THUMBS);
            String partThumb2 = Path.Combine(partThumb1, THB + fileName);
            if (File.Exists(partThumb2))
            {
                Bitmap thumpBitmap = new Bitmap(partThumb2);

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

            var result = new SavedImage(thumpBitmapWidth, thumpBitmapHeight, originalWidth, originalHeight, fileName);
            return result;
        }

        public void DeleteFiles(String pathToDelete)
        {
            string path = HostingEnvironment.MapPath(pathToDelete);

            System.Diagnostics.Debug.WriteLine(path);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    System.IO.File.Delete(fi.FullName);
                    System.Diagnostics.Debug.WriteLine(fi.Name);
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

        public String DeleteThumbFile(String file)
        {
            String partThumb1 = Path.Combine(StorageRoot, THUMBS);
            String partThumb2 = Path.Combine(partThumb1, THB + file);

            String succesMessage = "Error Delete";
            //delete thumb
            if (File.Exists(partThumb2))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(partThumb2);
                Thread.Sleep(100);
                succesMessage = "Ok";
            }
            return succesMessage;
        }

        public bool NormalFileExists(String file)
        {
            String fullPath = Path.Combine(StorageRoot, file);
            return File.Exists(fullPath);
        }

        public String DeleteNormalFile(String file)
        {
            String fullPath = Path.Combine(StorageRoot, file);
            String succesMessage = "Error Delete";
            if (File.Exists(fullPath))
            {
                //delete thumb
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(fullPath);
                Thread.Sleep(100);
                succesMessage = "Ok";
                return succesMessage;
            }
            return succesMessage;
        }

        public String DeleteFile(String file)
        {
            DeleteThumbFile(file);
            return DeleteNormalFile(file);
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
            var httpRequest = ContentBase.Request;
            // System.Diagnostics.Debug.WriteLine(Directory.Exists(tempPath));

            String fullPath = Path.Combine(StorageRoot);
            //Directory.CreateDirectory(fullPath);
            // Create new folder for thumbs
            //Directory.CreateDirectory(fullPath + "/thumbs/");

            foreach (String inputTagName in httpRequest.Files)
            {
                var headers = httpRequest.Headers;

                var file = httpRequest.Files[inputTagName];
                //System.Diagnostics.Debug.WriteLine(file.FileName);

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {
                    UploadWholeFile(ContentBase, resultList);
                }
                else
                {
                    //   UploadPartialFile(headers["X-File-Name"], ContentBase, resultList);
                }
            }
        }
       
        private SavedImage GetFileImageSize(int width, int height, byte[] fileByte)
        {
            Bitmap img = ByteArrayToBitmap(fileByte);
            return GetFileImageSize(width, height, img);
        }

        private SavedImage GetFileImageSize(int width, int height, Bitmap img)
        {
            int originalImageWidth = img.Width;
            int originalImageHeight = img.Height;
            double ratio = originalImageWidth.ToDouble() / originalImageHeight.ToDouble();
            img.Dispose();
            if (width == 0 && height > 0)
            {
                width = (int)(ratio * height);
            }
            else if (width > 0 && height == 0)
            {
                height = (int)((width * (1 / ratio)));
            }
            else if (width == 0 && height == 0)
            {
                //Create image from Bytes array
                height = height == 0 ? System.Convert.ToInt32(System.Convert.ToDouble(originalImageHeight) * .7) : height;
                width = width == 0 ? System.Convert.ToInt32(System.Convert.ToDouble(originalImageWidth) * .7) : width;
            }

            return new SavedImage(width, height, originalImageWidth, originalImageHeight);
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
                    var k = UploadResult(newFileName, file.ContentLength, newFileName, requestContext);
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
            String patchOnServer = Path.Combine(StorageRoot);
            var fullName = Path.Combine(patchOnServer, Path.GetFileName(FileName));
            Bitmap img = LoadImage(fullName);
            var result = new ViewDataUploadFilesResult()
            {
                name = FileName,
                size = fileSize,
                type = getType,
                width = img.Width,
                height = img.Height,
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
                if (extansion.Equals("jpeg") || extansion.Equals("jpg") || extansion.Equals("png") || extansion.Equals("gif"))
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
                    System.Diagnostics.Debug.WriteLine(fi.Name);
                }
            }
            return Filess;
        }

        public static bool IsMainImageExists(int? MainImageId, FileStorage MainImage)
        {
            if (MainImageId.HasValue && MainImage != null)
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
                        var fileByte = File.ReadAllBytes(fullPath);
                        var imageResize = GetFileImageSize(width, height, fileByte);
                        width = imageResize.Width;
                        height = imageResize.Height;
                        ImageFormat format = GetImageFormat(Path.GetExtension(mainImage.FileName));
                        var byteArrayIn = CreateThumbnail(fileByte, 90000, height, width, format);

                        DeleteThumbFile(mainImage.FileName);

                        using (Image thumbnail = ByteArrayToImage(byteArrayIn))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                thumbnail.Save(ms, format);
                                thumbnail.Save(candidatePathThb, format);
                            }
                        }
                        baseContent.ImageState = true;
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
            System.Diagnostics.Debug.WriteLine(fullPath);
            System.Diagnostics.Debug.WriteLine(System.IO.File.Exists(fullPath));
            String partThumb1 = Path.Combine(StorageRoot, THUMBS);
            String candidatePathThb = Path.Combine(partThumb1, THB + fileName);

            return new Tuple<string, string, string>(fullPath, candidatePathThb, fileName);
        }

        public SavedImage SaveImageByte(int width, int height, String fileName, String contentType, byte[] fileByte)
        {
            String fullPath = "", candidatePathThb = "", newFileName = "";
            int imageSize = 0;
            String fileHash = "";

            fileName = Path.GetFileName(fileName);
            var ext = Path.GetExtension(fileName);

            if (IsImage(ext))
            {
                var fileNames = GetFileNames(fileName);
                fullPath = fileNames.Item1;
                candidatePathThb = fileNames.Item2;
                newFileName = fileNames.Item3;

                var imageResize = GetFileImageSize(width, height, fileByte);
                width = imageResize.Width;
                height = imageResize.Height;
                int originalImageWidth = imageResize.OriginalWidth;
                int originalImageHeight = imageResize.OriginalHeight;

                fileHash = HashHelpers.GetSha256Hash(fileByte);

                var fileByteCropped = CreateThumbnail(fileByte, 90000, originalImageHeight, originalImageWidth, GetImageFormat(ext));

                imageSize = fileByteCropped.Length;

                using (Image thumbnail = ByteArrayToImage(fileByteCropped))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        thumbnail.Save(ms, GetImageFormat(ext));
                        thumbnail.Save(fullPath, GetImageFormat(ext));
                    }
                }

                var byteArrayIn = CreateThumbnail(fileByte, 90000, height, width, GetImageFormat(ext));

                using (Image thumbnail = ByteArrayToImage(byteArrayIn))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        thumbnail.Save(ms, GetImageFormat(ext));
                        thumbnail.Save(candidatePathThb, GetImageFormat(ext));
                    }
                }

                //saveWebPformat(fullPath, byteArrayIn);
            }
            else
            {
                fileHash = "Image Extension is not CORRECT:" + fileName;
                Logger.Error("Image Extension is not CORRECT:" + fileName);
            }

            return new SavedImage(newFileName, width, height, imageSize, contentType, fileName, fileHash);
        }

        private void saveWebPformat(string fullPath, byte[] byteArrayIn)
        {
            string webPFileName = Path.GetFileNameWithoutExtension(fullPath) + ".webp";
            string webPImagePath = Path.Combine(StorageRoot, webPFileName);
            // Then save in WebP format
            using (FileStream webPFileStream = new FileStream(webPImagePath, FileMode.Create))
            {
                ISupportedImageFormat lg_format = new WebPFormat { Quality = 100 };
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
        public Tuple<string, string> GetImageSrcPath(int fileStorageId)
        {
            var fileStorage = FileStorageService.GetFileStorage(fileStorageId);
            return Path.Combine(StorageRoot, fileStorage.FileName);
        }
        public byte[] GetFileStorageFromCache(int fileStorageId, out FileStorage fileStorage)
        {
            byte[] imageBytes = null;
            var cacheKeyFile = $"GetOriginalImageBytes-{fileStorageId}"; 
            fileStorage = FileStorageService.GetFileStorage(fileStorageId);
            if (fileStorage != null)
            {
                MemoryCacheProvider.Get(cacheKeyFile, out imageBytes);
                if (imageBytes == null)
                {
                    String fullPath = Path.Combine(StorageRoot, fileStorage.FileName);
                    if (File.Exists(fullPath))
                    {
                        imageBytes = File.ReadAllBytes(Path.Combine(fullPath));
                        MemoryCacheProvider.Set(cacheKeyFile, imageBytes, AppConfig.CacheLongSeconds);
                    }
                }
            }
            return imageBytes;
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

        public void CreateThumbnail(Bitmap startBitmap, int Width, int Height, String imageFullPath, ImageFormat format)
        {
            // create a new Bitmap with dimensions for the thumbnail.
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(Width, Height);

            // Copy the image from the START Bitmap into the NEW Bitmap.
            // This will create a thumnail size of the same image.
            newBitmap = ResizeImage(startBitmap, Width, Height);

            ConvertAndSaveBitmap(newBitmap, imageFullPath, format, 100);

            //var fs1 = new BinaryWriter(new FileStream(imageFullPath, FileMode.Append, FileAccess.Write));
            //fs1.Write(GetBitmapBytes(newBitmap));
            //fs1.Close();
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
            return ext == ".gif" || ext == ".jpg" || ext == ".png" || ext == ".bmp" || ext == ".tiff" || ext == ".jpe" || ext == ".jpeg";
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
            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders()[4];
            EncoderParameters parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

            //Temporary stream to save the bitmap
            using (MemoryStream tmpStream = new MemoryStream())
            {
                source.Save(tmpStream, codec, parameters);

                //Get image bytes from temporary stream
                byte[] result = new byte[tmpStream.Length];
                tmpStream.Seek(0, SeekOrigin.Begin);
                tmpStream.Read(result, 0, (int)tmpStream.Length);

                return result;
            }
        }

        private static ImageCodecInfo GetImageCodecInfo(string extension)
        {
            switch (extension)
            {
                case ".bmp": return ImageCodecInfo.GetImageEncoders()[0];
                case ".jpg": return ImageCodecInfo.GetImageEncoders()[1];
                case ".jpeg": return ImageCodecInfo.GetImageEncoders()[1];
                case ".gif": return ImageCodecInfo.GetImageEncoders()[2];
                case ".tiff": return ImageCodecInfo.GetImageEncoders()[3];
                case ".png": return ImageCodecInfo.GetImageEncoders()[4];
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

            // Get an ImageCodecInfo object that represents the JPEG codec.
            ImageCodecInfo imageCodecInfo = this.GetEncoderInfo(format);

            // Create an Encoder object for the Quality parameter.
            var encoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            EncoderParameters encoderParameters = new EncoderParameters(1);

            // Save the image as a JPEG file with quality level.
            EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
            encoderParameters.Param[0] = encoderParameter;
            newImage.Save(filePath, imageCodecInfo, encoderParameters);
        }

        /// <summary>
        /// Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }

        public static Bitmap LoadImage(string path)
        {
            var ms = new MemoryStream(File.ReadAllBytes(path));
            GC.KeepAlive(ms);
            return (Bitmap)Image.FromStream(ms);
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
            int emSize = 120;
            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(width, height))
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                //add question\
                var font = new Font("Tahoma", emSize);
                string color = "#F2F3F4";
                byte R = System.Convert.ToByte(color.Substring(1, 2), 16);
                byte G = System.Convert.ToByte(color.Substring(3, 2), 16);
                byte B = System.Convert.ToByte(color.Substring(5, 2), 16);
                Brush brush = new SolidBrush(Color.FromArgb(R, G, B));
                gfx.DrawString(text, font, brush, new Rectangle(0, 0, bmp.Width, bmp.Height), sf);
                //render as Jpeg
                bmp.Save(mem, ImageFormat.Jpeg);
                font.Dispose();
                brush.Dispose();
                sf.Dispose();
                return mem.GetBuffer();
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

                //add noise
                if (includenoise)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);

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

                //add question
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Black, 2, 3);

                //render as Jpeg
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);

                return mem.GetBuffer();
            }
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