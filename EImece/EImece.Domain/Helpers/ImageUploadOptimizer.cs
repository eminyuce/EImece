using EImece.Domain.Services.IServices;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Resize (never upscale) and re-encode uploaded catalog images so media/images/ does not
    /// store multi-megabyte originals when a smaller high-quality file is enough.
    /// </summary>
    public static class ImageUploadOptimizer
    {
        private static ILogger Logger =>
            Observability.Logging.LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(ImageUploadOptimizer))
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public const string MimeJpeg = MediaTypeNames.Image.Jpeg;
        public const string MimePng = "image/png";
        public const string MimeGif = MediaTypeNames.Image.Gif;
        public const string MimeWebP = "image/webp";
        public const string MimeBmp = "image/bmp";

        /// <summary>
        /// Scale down to fit inside maxWidth x maxHeight. Never upscales. A max of 0 or less means no cap on that axis.
        /// </summary>
        public static Size FitWithin(int sourceWidth, int sourceHeight, int maxWidth, int maxHeight)
        {
            if (sourceWidth <= 0 || sourceHeight <= 0)
            {
                return new Size(1, 1);
            }

            int targetW = sourceWidth;
            int targetH = sourceHeight;

            if (maxWidth > 0 && targetW > maxWidth)
            {
                targetH = (int)Math.Round(targetH * (maxWidth / (double)targetW));
                targetW = maxWidth;
            }

            if (maxHeight > 0 && targetH > maxHeight)
            {
                targetW = (int)Math.Round(targetW * (maxHeight / (double)targetH));
                targetH = maxHeight;
            }

            if (targetW < 1) targetW = 1;
            if (targetH < 1) targetH = 1;
            return new Size(targetW, targetH);
        }

        public static ImageOptimizationResult Optimize(byte[] source, ImageUploadOptimizeOptions options)
        {
            if (source == null || source.Length == 0)
            {
                throw new ArgumentException("Image bytes cannot be null or empty.", nameof(source));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.JpegQuality = ClampQuality(options.JpegQuality, 82);
            options.WebPQuality = ClampQuality(options.WebPQuality, 82);

            Bitmap loaded = null;
            Bitmap original = null;
            try
            {
                loaded = LoadBitmap(source);
                original = new Bitmap(loaded);
            }
            finally
            {
                if (loaded != null)
                {
                    loaded.Dispose();
                }
            }

            using (original)
            {
                int origW = original.Width;
                int origH = original.Height;
                Size target = FitWithin(origW, origH, options.MaxWidth, options.MaxHeight);
                bool hasAlpha = HasTransparency(original);
                string sourceExt = NormalizeExtension(options.SourceExtension);

                if (IsAnimatedGif(original))
                {
                    Logger.LogInformation("Skipping re-encode of animated GIF ({0} bytes, {1}x{2}) to preserve frames.", source.Length, origW, origH);
                    return KeepOriginal(source, origW, origH, sourceExt, options.SourceMimeType, MimeGif, ".gif");
                }

                using (Bitmap working = ResizeHighQuality(original, target.Width, target.Height, hasAlpha))
                {
                    EncodedImage standard = EncodeStandard(working, hasAlpha, options, sourceExt);
                    byte[] encoded = standard.Bytes;
                    string mime = standard.MimeType;
                    string ext = standard.Extension;

                    if (options.PreferWebP || string.Equals(options.ForceExtension, ".webp", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] webp = TryEncodeWebP(working, options.WebPQuality);
                        if (webp != null && webp.Length > 0 && (encoded == null || webp.Length < encoded.Length))
                        {
                            encoded = webp;
                            mime = MimeWebP;
                            ext = ".webp";
                        }
                    }

                    if (encoded == null || encoded.Length == 0)
                    {
                        throw new InvalidOperationException("Image encoding produced empty output.");
                    }

                    bool sameDimensions = target.Width == origW && target.Height == origH;
                    bool sameFormat = string.Equals(ext, sourceExt, StringComparison.OrdinalIgnoreCase)
                        || (IsJpegExtension(ext) && IsJpegExtension(sourceExt));
                    bool mustConvert = IsAlwaysConvertExtension(sourceExt);

                    if (options.KeepOriginalIfSmaller
                        && sameDimensions
                        && sameFormat
                        && !mustConvert
                        && encoded.Length >= source.Length)
                    {
                        Logger.LogDebug("Keeping original bytes; re-encode was not smaller ({0} -> {1}).", source.Length, encoded.Length);
                        return KeepOriginal(source, origW, origH, sourceExt, options.SourceMimeType, mime, ext);
                    }

                    return new ImageOptimizationResult
                    {
                        Bytes = encoded,
                        Width = working.Width,
                        Height = working.Height,
                        OriginalWidth = origW,
                        OriginalHeight = origH,
                        OriginalSize = source.Length,
                        MimeType = mime,
                        Extension = ext
                    };
                }
            }
        }

        public static int ClampQuality(int quality, int defaultValue)
        {
            if (quality <= 0)
            {
                return defaultValue;
            }

            if (quality < 40)
            {
                return 40;
            }

            if (quality > 95)
            {
                return 95;
            }

            return quality;
        }

        public static bool HasTransparency(byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return false;
            }

            using (Bitmap loaded = LoadBitmap(source))
            using (Bitmap clone = new Bitmap(loaded))
            {
                return HasTransparency(clone);
            }
        }

        private static ImageOptimizationResult KeepOriginal(
            byte[] source,
            int width,
            int height,
            string sourceExt,
            string sourceMime,
            string fallbackMime,
            string fallbackExt)
        {
            string ext = string.IsNullOrEmpty(sourceExt) ? fallbackExt : sourceExt;
            string mime = string.IsNullOrEmpty(sourceMime) ? MimeFromExtension(ext, fallbackMime) : sourceMime;
            return new ImageOptimizationResult
            {
                Bytes = source,
                Width = width,
                Height = height,
                OriginalWidth = width,
                OriginalHeight = height,
                OriginalSize = source.Length,
                MimeType = mime,
                Extension = ext,
                KeptOriginal = true
            };
        }

        private static EncodedImage EncodeStandard(Bitmap working, bool hasAlpha, ImageUploadOptimizeOptions options, string sourceExt)
        {
            string force = NormalizeExtension(options.ForceExtension);
            if (!string.IsNullOrEmpty(force) && !string.Equals(force, ".webp", StringComparison.OrdinalIgnoreCase))
            {
                if (IsJpegExtension(force))
                {
                    return new EncodedImage(EncodeJpeg(working, options.JpegQuality), MimeJpeg, ".jpg");
                }

                if (force == ".png")
                {
                    return new EncodedImage(EncodePng(working), MimePng, ".png");
                }

                if (force == ".gif")
                {
                    return new EncodedImage(EncodeGif(working), MimeGif, ".gif");
                }
            }

            if (hasAlpha)
            {
                return new EncodedImage(EncodePng(working), MimePng, ".png");
            }

            if (sourceExt == ".gif" && !hasAlpha)
            {
                return new EncodedImage(EncodeJpeg(working, options.JpegQuality), MimeJpeg, ".jpg");
            }

            if (sourceExt == ".png" && !hasAlpha)
            {
                byte[] jpegBytes = EncodeJpeg(working, options.JpegQuality);
                byte[] pngBytes = EncodePng(working);
                if (pngBytes.Length <= jpegBytes.Length)
                {
                    return new EncodedImage(pngBytes, MimePng, ".png");
                }

                return new EncodedImage(jpegBytes, MimeJpeg, ".jpg");
            }

            if (IsJpegExtension(sourceExt) || sourceExt == ".bmp" || sourceExt == ".tif" || sourceExt == ".tiff")
            {
                return new EncodedImage(EncodeJpeg(working, options.JpegQuality), MimeJpeg, ".jpg");
            }

            return new EncodedImage(EncodeJpeg(working, options.JpegQuality), MimeJpeg, ".jpg");
        }

        private static byte[] EncodeJpeg(Bitmap source, int quality)
        {
            using (Bitmap rgb = FlattenTo24bpp(source))
            using (MemoryStream ms = new MemoryStream())
            {
                ImageCodecInfo codec = GetEncoder(ImageFormat.Jpeg);
                using (EncoderParameters encoderParams = new EncoderParameters(1))
                {
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                    rgb.Save(ms, codec, encoderParams);
                }

                return ms.ToArray();
            }
        }

        private static byte[] EncodePng(Bitmap source)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ImageCodecInfo codec = GetEncoder(ImageFormat.Png);
                if (codec != null)
                {
                    using (EncoderParameters encoderParams = new EncoderParameters(1))
                    {
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        source.Save(ms, codec, encoderParams);
                    }
                }
                else
                {
                    source.Save(ms, ImageFormat.Png);
                }

                return ms.ToArray();
            }
        }

        private static byte[] EncodeGif(Bitmap source)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                source.Save(ms, ImageFormat.Gif);
                return ms.ToArray();
            }
        }

        private static byte[] TryEncodeWebP(Bitmap bitmap, int quality)
        {
            try
            {
                using (MemoryStream inStream = new MemoryStream())
                {
                    bitmap.Save(inStream, ImageFormat.Png);
                    inStream.Position = 0;
                    using (MemoryStream outStream = new MemoryStream())
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        ISupportedImageFormat webPFormat = new WebPFormat { Quality = quality };
                        imageFactory.Load(inStream)
                            .Format(webPFormat)
                            .Save(outStream);
                        return outStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "WebP encode failed; using JPEG/PNG instead.");
                return null;
            }
        }

        private static Bitmap ResizeHighQuality(Bitmap source, int width, int height, bool preserveAlpha)
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            PixelFormat format = preserveAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;
            Bitmap dest = new Bitmap(width, height, format);
            dest.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (Graphics gfx = Graphics.FromImage(dest))
            {
                gfx.CompositingMode = preserveAlpha ? CompositingMode.SourceCopy : CompositingMode.SourceOver;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                if (!preserveAlpha)
                {
                    gfx.Clear(Color.White);
                }
                else
                {
                    gfx.Clear(Color.Transparent);
                }

                gfx.DrawImage(source, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, source.Width, source.Height), GraphicsUnit.Pixel);
            }

            return dest;
        }

        private static Bitmap FlattenTo24bpp(Bitmap source)
        {
            if (source.PixelFormat == PixelFormat.Format24bppRgb)
            {
                return new Bitmap(source);
            }

            Bitmap dest = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using (Graphics gfx = Graphics.FromImage(dest))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            return dest;
        }

        private static Bitmap LoadBitmap(byte[] source)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(source))
                using (Image img = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: false))
                {
                    return new Bitmap(img);
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "GDI+ failed to load image; trying ImageProcessor.");
            }

            try
            {
                using (ImageFactory factory = new ImageFactory(preserveExifData: false))
                {
                    factory.Load(source);
                    return new Bitmap(factory.Image);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not decode the uploaded image.", ex);
            }
        }

        internal static bool HasTransparency(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return false;
            }

            if (bitmap.Palette != null && bitmap.Palette.Entries != null && bitmap.Palette.Entries.Length > 0)
            {
                foreach (Color entry in bitmap.Palette.Entries)
                {
                    if (entry.A < 255)
                    {
                        return true;
                    }
                }
            }

            if (!Image.IsAlphaPixelFormat(bitmap.PixelFormat))
            {
                return false;
            }

            BitmapData data = null;
            try
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int length = Math.Abs(data.Stride) * bitmap.Height;
                byte[] pixels = new byte[length];
                Marshal.Copy(data.Scan0, pixels, 0, length);
                for (int i = 3; i < pixels.Length; i += 4)
                {
                    if (pixels[i] < 255)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                if (data != null)
                {
                    bitmap.UnlockBits(data);
                }
            }

            return false;
        }

        private static bool IsAnimatedGif(Image image)
        {
            try
            {
                if (image.RawFormat.Guid != ImageFormat.Gif.Guid)
                {
                    return false;
                }

                return image.GetFrameCount(FrameDimension.Time) > 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == format.Guid);
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }

            extension = extension.Trim().ToLowerInvariant();
            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = "." + extension;
            }

            if (extension == ".jpeg" || extension == ".jpe")
            {
                return ".jpg";
            }

            return extension;
        }

        private static bool IsJpegExtension(string ext)
        {
            return ext == ".jpg" || ext == ".jpeg" || ext == ".jpe";
        }

        private static bool IsAlwaysConvertExtension(string ext)
        {
            return ext == ".bmp" || ext == ".tif" || ext == ".tiff";
        }

        private static string MimeFromExtension(string ext, string fallback)
        {
            switch (NormalizeExtension(ext))
            {
                case ".jpg":
                    return MimeJpeg;
                case ".png":
                    return MimePng;
                case ".gif":
                    return MimeGif;
                case ".webp":
                    return MimeWebP;
                case ".bmp":
                    return MimeBmp;
                default:
                    return fallback;
            }
        }

        private sealed class EncodedImage
        {
            public EncodedImage(byte[] bytes, string mimeType, string extension)
            {
                Bytes = bytes;
                MimeType = mimeType;
                Extension = extension;
            }

            public byte[] Bytes { get; private set; }
            public string MimeType { get; private set; }
            public string Extension { get; private set; }
        }
    }

    public class ImageUploadOptimizeOptions
    {
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public int JpegQuality { get; set; }
        public int WebPQuality { get; set; }
        public bool PreferWebP { get; set; }
        public bool KeepOriginalIfSmaller { get; set; }
        public string SourceExtension { get; set; }
        public string SourceMimeType { get; set; }

        /// <summary>
        /// When set (e.g. thumbnail matching the stored full image), encode as this extension.
        /// </summary>
        public string ForceExtension { get; set; }

        private static int GetSettingInt(ISettingService settingService, string key, int defaultValue)
        {
            var val = settingService?.GetSettingByKey(key);
            return !string.IsNullOrWhiteSpace(val) ? val.ToInt(defaultValue) : defaultValue;
        }

        private static bool GetSettingBool(ISettingService settingService, string key, bool defaultValue)
        {
            var val = settingService?.GetSettingByKey(key);
            return !string.IsNullOrWhiteSpace(val) ? val.ToBool(defaultValue) : defaultValue;
        }

        public static ImageUploadOptimizeOptions ForFullImage(string fileName, string contentType, ISettingService settingService = null)
        {
            return new ImageUploadOptimizeOptions
            {
                MaxWidth = GetSettingInt(settingService, Constants.ImageUploadMaxWidth, Constants.DefaultImageUploadMaxWidth),
                MaxHeight = GetSettingInt(settingService, Constants.ImageUploadMaxHeight, Constants.DefaultImageUploadMaxHeight),
                JpegQuality = ImageUploadOptimizer.ClampQuality(GetSettingInt(settingService, Constants.ImageUploadJpegQuality, Constants.DefaultImageUploadJpegQuality), Constants.DefaultImageUploadJpegQuality),
                WebPQuality = ImageUploadOptimizer.ClampQuality(GetSettingInt(settingService, Constants.ImageUploadWebPQuality, Constants.DefaultImageUploadWebPQuality), Constants.DefaultImageUploadWebPQuality),
                PreferWebP = GetSettingBool(settingService, Constants.ImageUploadPreferWebP, Constants.DefaultImageUploadPreferWebP),
                KeepOriginalIfSmaller = GetSettingBool(settingService, Constants.ImageUploadKeepOriginalIfSmaller, Constants.DefaultImageUploadKeepOriginalIfSmaller),
                SourceExtension = Path.GetExtension(fileName),
                SourceMimeType = contentType
            };
        }

        public static ImageUploadOptimizeOptions ForThumbnail(string fileName, string contentType, int maxWidth, int maxHeight, string forceExtension, ISettingService settingService = null)
        {
            return new ImageUploadOptimizeOptions
            {
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                JpegQuality = ImageUploadOptimizer.ClampQuality(GetSettingInt(settingService, Constants.ImageUploadThumbJpegQuality, Constants.DefaultImageUploadThumbJpegQuality), Constants.DefaultImageUploadThumbJpegQuality),
                WebPQuality = ImageUploadOptimizer.ClampQuality(GetSettingInt(settingService, Constants.ImageUploadWebPQuality, Constants.DefaultImageUploadWebPQuality), Constants.DefaultImageUploadWebPQuality),
                PreferWebP = string.Equals(forceExtension, ".webp", StringComparison.OrdinalIgnoreCase),
                KeepOriginalIfSmaller = false,
                SourceExtension = Path.GetExtension(fileName),
                SourceMimeType = contentType,
                ForceExtension = forceExtension
            };
        }
    }

    public class ImageOptimizationResult
    {
        public byte[] Bytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int OriginalSize { get; set; }
        public string MimeType { get; set; }
        public string Extension { get; set; }
        public bool KeptOriginal { get; set; }

        public bool IsWebP
        {
            get { return string.Equals(MimeType, ImageUploadOptimizer.MimeWebP, StringComparison.OrdinalIgnoreCase); }
        }
    }
}
