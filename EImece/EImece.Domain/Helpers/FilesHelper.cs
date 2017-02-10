using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using EImece.Domain.Caching;
using NLog;

namespace EImece.Domain.Helpers
{
    public class FilesHelper
    {
        [Inject]
        public IFileStorageService FileStorageService { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public bool IsCachingActive { get; set; }
        private ICacheProvider _memoryCacheProvider { get; set; }
        [Inject]
        public ICacheProvider MemoryCacheProvider
        {
            get
            {
                _memoryCacheProvider.IsCacheProviderActive = IsCachingActive;
                return _memoryCacheProvider;
            }
            set
            {
                _memoryCacheProvider = value;
            }
        }


        public String DeleteURL { get; set; }
        public String DeleteType { get; set; }
        public String StorageRoot { get; set; }
        public String UrlBase { get; set; }
        public String tempPath { get; set; }
        //ex:"~/Files/something/";
        public String serverMapPath { get; set; }
        public void Init(String DeleteURL, String DeleteType, String StorageRoot, String UrlBase, String tempPath, String serverMapPath)
        {
            this.DeleteURL = DeleteURL;
            this.DeleteType = DeleteType;
            this.StorageRoot = StorageRoot;
            this.UrlBase = UrlBase;
            this.tempPath = tempPath;
            this.serverMapPath = serverMapPath;
        }
        public Tuple<int, int, int, int, string> GetThumbnailImageSize(int mainPageId)
        {
            var mainPage = FileStorageService.GetSingle(mainPageId);
            return GetThumbnailImageSize(mainPage);
        }
        public Tuple<int, int, int, int, string> GetThumbnailImageSize(FileStorage mainImage)
        {
            int thumpBitmapWidth = 0, thumpBitmapHeight = 0;
            int originalWidth = 0, originalHeight = 0;
            String fileName = "";
            if (mainImage != null)
            {
                fileName = mainImage.FileName;
                return GetThumbnailImageSize(fileName);
            }
            var result = new Tuple<int, int, int, int, string>(thumpBitmapWidth, thumpBitmapHeight, originalWidth, originalHeight, fileName);
            return result;
        }
        public Tuple<int, int, int, int, string> GetThumbnailImageSize(String fileName)
        {
            int thumpBitmapWidth = 0, thumpBitmapHeight = 0;
            int originalWidth = 0, originalHeight = 0;
            String fullPath = Path.Combine(StorageRoot, fileName);
            String thumbPath = "/thb" + fileName + "";
            String partThumb1 = Path.Combine(StorageRoot, "thumbs");
            String partThumb2 = Path.Combine(partThumb1, "thb" + fileName);
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

            var result = new Tuple<int, int, int, int, string>(thumpBitmapWidth, thumpBitmapHeight, originalWidth, originalHeight, fileName);
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
            var fileResult = DeleteFile(file);
            if (fileResult.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                var request = ContentBase.Request;
                int contentId = request.QueryString["contentId"].ToInt();
                var imageType = EnumHelper.Parse<EImeceImageType>(request.QueryString["imageType"].ToStr());
                var mod = EnumHelper.Parse<MediaModType>(request.QueryString["mod"].ToStr());

                FileStorageService.DeleteUploadImage(file, contentId, imageType, mod);
            }

            return fileResult;
        }

        public String DeleteThumbFile(String file)
        {
            String partThumb1 = Path.Combine(StorageRoot, "thumbs");
            String partThumb2 = Path.Combine(partThumb1, "thb" + file);

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
            System.Diagnostics.Debug.WriteLine(Directory.Exists(tempPath));

            String fullPath = Path.Combine(StorageRoot);
            Directory.CreateDirectory(fullPath);
            // Create new folder for thumbs
            Directory.CreateDirectory(fullPath + "/thumbs/");

            foreach (String inputTagName in httpRequest.Files)
            {

                var headers = httpRequest.Headers;

                var file = httpRequest.Files[inputTagName];
                System.Diagnostics.Debug.WriteLine(file.FileName);

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {

                    UploadWholeFile(ContentBase, resultList);
                }
                else
                {

                    UploadPartialFile(headers["X-File-Name"], ContentBase, resultList);
                }
            }
        }
        private Tuple<int, int, int, int> GetFileImageSize(int width, int height, byte[] fileByte)
        {
            Bitmap img = ByteArrayToBitmap(fileByte);
            return GetFileImageSize(width, height, img);
        }
        private Tuple<int, int, int, int> GetFileImageSize(int width, int height, Bitmap img)
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
            else if (width > 0 && height > 0)
            {

            }

            return new Tuple<int, int, int, int>(width, height, originalImageWidth, originalImageHeight);
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
                    var newFileName = result.Item1;
                    statuses.Add(UploadResult(newFileName, file.ContentLength, newFileName, requestContext));
                }
            }
        }



        private void UploadPartialFile(string fileName, HttpContextBase requestContext, List<ViewDataUploadFilesResult> statuses)
        {
            var request = requestContext.Request;
            if (request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var file = request.Files[0];
            var inputStream = file.InputStream;
            String patchOnServer = Path.Combine(StorageRoot);
            var fullName = Path.Combine(patchOnServer, Path.GetFileName(file.FileName));
            var ThumbfullPath = Path.Combine(fullName, Path.GetFileName(file.FileName + ".80x80.jpg"));


            var ImageBit = LoadImage(fullName);
            Save(ImageBit, 80, 80, 10, ThumbfullPath);
            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(UploadResult(file.FileName, file.ContentLength, file.FileName, requestContext));
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
            string path = HostingEnvironment.MapPath(serverMapPath);
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

        public void SaveFileFromHttpPostedFileBase(HttpPostedFileBase httpPostedFileBase,
            int height = 0,
            int width = 0,
            EImeceImageType imageType = EImeceImageType.NONE, BaseContent baseContent = null)
        {
            if (httpPostedFileBase != null)
            {
                var result = SaveImageByte(width, height, httpPostedFileBase);

                var fileStorage = new FileStorage();
                fileStorage.Name = result.Item6;
                fileStorage.FileName = result.Item1;
                fileStorage.Width = result.Item2;
                fileStorage.Height = result.Item3;
                fileStorage.MimeType = result.Item5;
                fileStorage.CreatedDate = DateTime.Now;
                fileStorage.UpdatedDate = DateTime.Now;
                fileStorage.IsActive = true;
                fileStorage.Position = 1;
                fileStorage.FileSize = result.Item4;
                fileStorage.Type = imageType.ToStr();

                FileStorageService.SaveOrEditEntity(fileStorage);
                baseContent.MainImageId = fileStorage.Id;

            }
            else
            {
                if (baseContent.MainImageId.HasValue && baseContent.MainImageId.Value > 0)
                {
                    var mainImage = FileStorageService.GetSingle(baseContent.MainImageId.Value);
                    if (mainImage != null)
                    {
                        var imageSize = GetThumbnailImageSize(mainImage);
                        int mainImageHeight = imageSize.Item2;
                        int mainImageWidth = imageSize.Item1;
                        if (mainImageHeight != height && mainImageWidth != width) //Resize thumb image with new dimension.
                        {

                            DeleteThumbFile(mainImage.FileName);
                            String fullPath = Path.Combine(StorageRoot, mainImage.FileName);
                            String partThumb1 = Path.Combine(StorageRoot, "thumbs");
                            String candidatePathThb = Path.Combine(partThumb1, "thb" + mainImage.FileName);

                            var fileByte = File.ReadAllBytes(fullPath);
                            var ext = Path.GetExtension(mainImage.FileName);
                            var imageResize = GetFileImageSize(width, height, fileByte);
                            width = imageResize.Item1;
                            height = imageResize.Item2;
                            int originalImageWidth = imageResize.Item3;
                            int originalImageHeight = imageResize.Item4;

                            var byteArrayIn = CreateThumbnail(fileByte, 90000, height, width, GetImageFormat(ext));
                            var fs1 = new BinaryWriter(new FileStream(candidatePathThb, FileMode.Append, FileAccess.Write));
                            fs1.Write(byteArrayIn);
                            fs1.Close();

                        }
                    }
                }
            }
        }
        private Tuple<string, string, string> GetFileNames(String fileName)
        {
            var ext = Path.GetExtension(fileName);
            var fileBase = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_");
            Random random = new Random();
            var randomNumber = random.Next(0, int.MaxValue).ToString();
            var newFileName = string.Format(@"{0}_{1}{2}", fileBase, randomNumber, ext);

            String fullPath = Path.Combine(StorageRoot, newFileName);
            System.Diagnostics.Debug.WriteLine(fullPath);
            System.Diagnostics.Debug.WriteLine(System.IO.File.Exists(fullPath));
            String partThumb1 = Path.Combine(StorageRoot, "thumbs");
            String candidatePathThb = Path.Combine(partThumb1, "thb" + newFileName);

            return new Tuple<string, string, string>(fullPath, candidatePathThb, newFileName);
        }
        public Tuple<string, int, int, int, string, string> SaveImageByte(int width, int height, HttpPostedFileBase file)
        {
            String fullPath = "", candidatePathThb = "", newFileName = "";
            int imageSize = 0;

            String fileName = "";
            String contentType = file.ContentType;

            fileName = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(fileName);

            if (IsImage(ext))
            {
                var fileByte = GeneralHelper.ReadFully(file.InputStream);
                var fileNames = GetFileNames(fileName);
                fullPath = fileNames.Item1;
                candidatePathThb = fileNames.Item2;
                newFileName = fileNames.Item3;


                var imageResize = GetFileImageSize(width, height, fileByte);
                width = imageResize.Item1;
                height = imageResize.Item2;
                int originalImageWidth = imageResize.Item3;
                int originalImageHeight = imageResize.Item4;


                var fileByteCropped = CreateThumbnail(fileByte, 90000, originalImageHeight, originalImageWidth, GetImageFormat(ext));
                var fs = new BinaryWriter(new FileStream(fullPath, FileMode.Append, FileAccess.Write));
                fs.Write(fileByteCropped);
                fs.Close();

                imageSize = fileByteCropped.Length;

                //Resize Image - Thumbs
                var byteArrayIn = CreateThumbnail(fileByte, 90000, height, width, GetImageFormat(ext));
                var fs1 = new BinaryWriter(new FileStream(candidatePathThb, FileMode.Append, FileAccess.Write));
                fs1.Write(byteArrayIn);
                fs1.Close();


            }

            return new Tuple<string, int, int, int, string, string>(newFileName, width, height, imageSize, contentType, fileName);

        }

        public ImageFormat GetImageFormat(String extension)
        {
            extension = extension.Replace(".", "");
            switch (extension)
            {
                case "jpeg": return ImageFormat.Jpeg;
                case "jpg": return ImageFormat.Jpeg;
                case "png":  return ImageFormat.Png; 
                case "icon": return ImageFormat.Icon; 
                case "gif": return ImageFormat.Gif;
                case "bmp": return ImageFormat.Bmp; 
                case "tiff": return ImageFormat.Tiff; 
                case "emf": return ImageFormat.Emf;
                case "wmf": return ImageFormat.Wmf; 
            }

            return ImageFormat.Jpeg;
        }

        public ImageOrientation GetOrientation(int width, int height)
        {
            if (width == 0 || height == 0)
                return ImageOrientation.Unknown;

            float relation = (float)height / (float)width;

            if (relation > .95 && relation < 1.05)
            {
                return ImageOrientation.Square;
            }
            else if (relation > 1.05 && relation < 2)
            {
                return ImageOrientation.Portrate;
            }
            else if (relation >= 2)
            {
                return ImageOrientation.Vertical;
            }
            else if (relation <= .95 && relation > .5)
            {
                return ImageOrientation.Landscape;
            }
            else if (relation <= .5)
            {
                return ImageOrientation.Horizontal;
            }
            else
            {
                return ImageOrientation.Unknown;
            }
        }
        public byte[] GetResizedImage(int fileStorageId, int width, int height)
        {
            var cacheKey = String.Format("GetResizedImage-{0}-{1}-{2}", fileStorageId, width,height);
            byte[] result = null;
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                var fileStorage = FileStorageService.GetSingle(fileStorageId);
                if (fileStorage != null)
                {
                    var file = fileStorage.FileName;
                    String fullPath = Path.Combine(StorageRoot, file);
                    if (System.IO.File.Exists(fullPath))
                    {
                        var fullImagePath = Path.Combine(fullPath);
                        Bitmap b = new Bitmap(fullImagePath);

                        var imageResize = GetFileImageSize(width, height, b);
                        width = imageResize.Item1;
                        height = imageResize.Item2;
                        int originalImageWidth = imageResize.Item3;
                        int originalImageHeight = imageResize.Item4;
                        b.Dispose();

                        b = new Bitmap(fullImagePath);
                        var resizeBitmap = ResizeImage(b, width, height);
                        result = GetBitmapBytes(resizeBitmap);
                        b.Dispose();
                        resizeBitmap.Dispose();
                        MemoryCacheProvider.Set(cacheKey, result, Settings.CacheShortSeconds);
                    }
                }
            }
            return result;
          
        }
        public byte[] MakeThumbnail(byte[] myImage, int thumbWidth, int thumbHeight)
        {
            using (MemoryStream ms = new MemoryStream())
            using (Image thumbnail = Image.FromStream(new MemoryStream(myImage)).GetThumbnailImage(thumbWidth, thumbHeight, null, new IntPtr()))
            {
                thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
        public Bitmap Crop(Bitmap bitmap, int Width, int Height, AnchorPosition Anchor)
        {

            int sourceWidth = bitmap.Width;
            int sourceHeight = bitmap.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (Anchor)
                {
                    case AnchorPosition.Top:
                        destY = 0;
                        break;
                    case AnchorPosition.Bottom:
                        destY = (int)(Height - (sourceHeight * nPercent));
                        break;
                    default:
                        destY = (int)((Height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (Anchor)
                {
                    case AnchorPosition.Left:
                        destX = 0;
                        break;
                    case AnchorPosition.Right:
                        destX = (int)(Width - (sourceWidth * nPercent));
                        break;
                    default:
                        destX = (int)((Width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format48bppRgb);
            bmPhoto.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);

            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;

            grPhoto.DrawImage(bitmap,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);


            grPhoto.Dispose();
            return bmPhoto;
        }
        // Create a thumbnail in byte array format from the image encoded in the passed byte array.  
        // (RESIZE an image in a byte[] variable.)  
        public byte[] CreateThumbnail(byte[] PassedImage, int LargestSide, int Height, int Width, ImageFormat format)
        {
            byte[] ReturnedThumbnail;

            using (System.IO.MemoryStream StartMemoryStream = new System.IO.MemoryStream(), NewMemoryStream = new System.IO.MemoryStream())
            {
                // write the string to the stream  
                StartMemoryStream.Write(PassedImage, 0, PassedImage.Length);

                // create the start Bitmap from the MemoryStream that contains the image  
                System.Drawing.Bitmap startBitmap = new System.Drawing.Bitmap(StartMemoryStream);

                // set thumbnail height and width proportional to the original image.  
                int newHeight;
                int newWidth;
                double HW_ratio;
                if (startBitmap.Height > startBitmap.Width)
                {
                    newHeight = LargestSide;
                    HW_ratio = (double)((double)LargestSide / (double)startBitmap.Height);
                    newWidth = (int)(HW_ratio * (double)startBitmap.Width);
                }
                else
                {
                    newWidth = LargestSide;
                    HW_ratio = (double)((double)LargestSide / (double)startBitmap.Width);
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



        // Resize a Bitmap  
        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }
        public static Bitmap ConvertAndSaveBitmap(Bitmap bitmap, String fileName, ImageFormat imageFormat, long quality = 100L)
        {
            using (var encoderParameters = new EncoderParameters(1))
            using (encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality))
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

                bitmap.Save(fileName, codecs.Single(codec => codec.FormatID == imageFormat.Guid), encoderParameters);
            }

            return bitmap;
        }

        public static bool IsImage(string ext)
        {
            return ext == ".gif" || ext == ".jpg" || ext == ".png" || ext == ".bmp" || ext == ".tiff" || ext == ".jpe";
        }
        /// <summary>
        /// Determines if a file is a known image type by checking the extension.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file is an image.</returns>
        public static bool IsImageByFileName(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            string types = "|.png|.gif|.jpg|.jpeg|.bmp|.tiff|";

            return types.IndexOf("|" + ext + "|") >= 0;
        }
        private string EncodeFile(string fileName)
        {
            return System.Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        static double ConvertBytesToMegabytes(long bytes)
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

       

        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>      
        public void Save(Bitmap image, int maxWidth, int maxHeight, int quality, string filePath)
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
            ImageCodecInfo imageCodecInfo = this.GetEncoderInfo(ImageFormat.Jpeg);

            // Create an Encoder object for the Quality parameter.
            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

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
    public enum ImageOrientation
    {
        Unknown = 0,
        Horizontal = 8,
        Landscape = 9,
        Square = 10,
        Portrate = 11,
        Vertical = 12
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
