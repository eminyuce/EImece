using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace EImece.Domain.Helpers
{
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
    public class ImageHelper
    {
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
        public static byte[] MakeThumbnail(byte[] myImage, int thumbWidth, int thumbHeight)
        {
            using (MemoryStream ms = new MemoryStream())
            using (Image thumbnail = Image.FromStream(new MemoryStream(myImage)).GetThumbnailImage(thumbWidth, thumbHeight, null, new IntPtr()))
            {
                thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public static Bitmap Crop(Bitmap bitmap, int Width, int Height, AnchorPosition Anchor)
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
        public static byte[] CreateThumbnail(byte[] PassedImage, int LargestSide, int Height, int Width)
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
                newBitmap.Save(NewMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                // Fill the byte[] for the thumbnail from the new MemoryStream.  
                ReturnedThumbnail = NewMemoryStream.ToArray();
            }
            // return the resized image as a string of bytes.  
            return ReturnedThumbnail;
        }


        public static byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }




        // Resize a Bitmap  
        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }


        public static bool IsImage(string ext)
        {
            return ext == ".gif" || ext == ".jpg" || ext == ".png";
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
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

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

        public static byte[] CreateGoogleImage(HttpPostedFileBase file)
        {

            var fileByte = GeneralHelper.ReadFully(file.InputStream);
            //Create image from Bytes array
            System.Drawing.Image img = ByteArrayToImage(fileByte);
            int height = System.Convert.ToInt32(System.Convert.ToDouble(img.Height) * .7);
            int width = System.Convert.ToInt32(System.Convert.ToDouble(img.Width) * .7);
            //Resize Image - ORIGINAL
            var byteArrayIn = CreateThumbnail(fileByte, 10000, height, width);

            return byteArrayIn;
        }
        public static FileStorage SaveFileFromUrl(string url, int height = 0, int width = 0, EImeceImageType imageType = EImeceImageType.NONE)
        {
            var dictionary = new Dictionary<string, string>();
            var fileByte = DownloadHelper.GetImageFromUrl(url, dictionary);
            String fileName = dictionary["FileName"];
            String contentType = dictionary["ContentType"];
            return SaveImageByte(ref height, ref width, fileName, contentType, fileByte, imageType);
        }

        public static FileStorage SaveFileFromHttpPostedFileBase(HttpPostedFileBase file,
            int height = 0, 
            int width = 0,
            EImeceImageType imageType = EImeceImageType.NONE)
        {
            String fileName = file.FileName;
            String contentType = file.ContentType;

            fileName = Path.GetFileName(file.FileName);

            var fileByte = GeneralHelper.ReadFully(file.InputStream);

            return SaveImageByte(ref height, ref width, fileName, contentType, fileByte, imageType);
        }

        private static FileStorage SaveImageByte(ref int height, ref int width, string fileName, string contentType, byte[] fileByte, EImeceImageType imageType)
        {
            var fileStorage = new FileStorage();
            if (IsImageByFileName(fileName))
            {
                var ext = Path.GetExtension(fileName);
                var fileBase = Path.GetFileNameWithoutExtension(fileName);
                string url = HttpContext.Current.Server.MapPath("~/media/images");

                Random random = new Random();
                var randomNumber = random.Next(0, int.MaxValue).ToString();
                var newFileName = string.Format(@"{0}_{1}{2}", fileBase, randomNumber, ext);
                var candidatePath = string.Format(@"{0}\{1}", url, newFileName);
                var candidatePathThb = string.Format(@"{0}\thb{1}", url, newFileName);

                if (!File.Exists(candidatePath))
                {
                    System.Drawing.Image img = ByteArrayToImage(fileByte);

                    // var fileBitMap = Crop(new Bitmap(img), img.Height, img.Width, AnchorPosition.Center);
                    // var byteArrayCroppped = GetBitmapBytes(fileBitMap);
                    var fileByteCropped = CreateThumbnail(fileByte, 90000, img.Height, img.Width);
                    var fs = new BinaryWriter(new FileStream(candidatePath, FileMode.Append, FileAccess.Write));
                    fs.Write(fileByteCropped);
                    fs.Close();


                    //Create image from Bytes array
                    height = height == 0 ? System.Convert.ToInt32(System.Convert.ToDouble(img.Height) * .7) : height;
                    width = width == 0 ? System.Convert.ToInt32(System.Convert.ToDouble(img.Width) * .7) : width;
                    //Resize Image - ORIGINAL
                    var byteArrayIn = CreateThumbnail(fileByte, 10000, height, width);


                    var fs1 = new BinaryWriter(new FileStream(candidatePathThb, FileMode.Append, FileAccess.Write));
                    fs1.Write(byteArrayIn);
                    fs1.Close();


                    fileStorage.Name = fileName;
                    fileStorage.FileName = newFileName;
                    fileStorage.Width = width;
                    fileStorage.Height = height;
                    fileStorage.MimeType = contentType;
                    fileStorage.CreatedDate = DateTime.Now;
                    fileStorage.UpdatedDate = DateTime.Now;
                    fileStorage.IsActive = true;
                    fileStorage.Position = 1;
                    fileStorage.FileSize = fileByteCropped.Length;
                    fileStorage.Type = imageType.ToStr();

                }

            }
            return fileStorage;
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
}
