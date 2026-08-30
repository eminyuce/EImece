using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ImageUploadOptimizerTests
    {
        [TestMethod]
        public void FitWithin_DoesNotUpscale()
        {
            var size = ImageUploadOptimizer.FitWithin(800, 600, 1920, 1920);
            Assert.AreEqual(800, size.Width);
            Assert.AreEqual(600, size.Height);
        }

        [TestMethod]
        public void FitWithin_CapsMaxWidthPreservingAspectRatio()
        {
            var size = ImageUploadOptimizer.FitWithin(4000, 3000, 1920, 1920);
            Assert.AreEqual(1920, size.Width);
            Assert.AreEqual(1440, size.Height);
        }

        [TestMethod]
        public void FitWithin_CapsMaxHeightPreservingAspectRatio()
        {
            var size = ImageUploadOptimizer.FitWithin(1000, 4000, 1920, 1920);
            Assert.AreEqual(480, size.Width);
            Assert.AreEqual(1920, size.Height);
        }

        [TestMethod]
        public void FitWithin_ZeroMaxMeansNoCapOnThatAxis()
        {
            var size = ImageUploadOptimizer.FitWithin(4000, 200, 1920, 0);
            Assert.AreEqual(1920, size.Width);
            Assert.AreEqual(96, size.Height);
        }

        [TestMethod]
        public void ClampQuality_UsesDefaultAndBounds()
        {
            Assert.AreEqual(82, ImageUploadOptimizer.ClampQuality(0, 82));
            Assert.AreEqual(40, ImageUploadOptimizer.ClampQuality(10, 82));
            Assert.AreEqual(95, ImageUploadOptimizer.ClampQuality(100, 82));
            Assert.AreEqual(82, ImageUploadOptimizer.ClampQuality(82, 75));
        }

        [TestMethod]
        public void Optimize_DownsizesLargeJpegAndReducesBytes()
        {
            byte[] source = CreateJpeg(2500, 1800, 95L);
            var result = ImageUploadOptimizer.Optimize(source, FullOptions(".jpg"));

            Assert.IsTrue(result.Width <= 1920, "width should be capped: " + result.Width);
            Assert.IsTrue(result.Height <= 1920, "height should be capped: " + result.Height);
            Assert.AreEqual(2500, result.OriginalWidth);
            Assert.AreEqual(1800, result.OriginalHeight);
            Assert.IsTrue(result.Width < 2500);
            Assert.AreEqual(ImageUploadOptimizer.MimeJpeg, result.MimeType);
            Assert.AreEqual(".jpg", result.Extension);
            Assert.IsTrue(result.Bytes.Length < source.Length, string.Format("stored {0} should be smaller than original {1}", result.Bytes.Length, source.Length));

            using (var ms = new MemoryStream(result.Bytes))
            using (var img = Image.FromStream(ms))
            {
                Assert.AreEqual(result.Width, img.Width);
                Assert.AreEqual(result.Height, img.Height);
            }
        }

        [TestMethod]
        public void Optimize_DoesNotUpscaleSmallJpeg()
        {
            byte[] source = CreateJpeg(400, 300, 90L);
            var result = ImageUploadOptimizer.Optimize(source, FullOptions(".jpg"));

            Assert.AreEqual(400, result.Width);
            Assert.AreEqual(300, result.Height);
        }

        [TestMethod]
        public void Optimize_ConvertsBmpToSmallerJpeg()
        {
            byte[] source = CreateBmp(640, 480);
            var result = ImageUploadOptimizer.Optimize(source, FullOptions(".bmp", "image/bmp"));

            Assert.AreEqual(ImageUploadOptimizer.MimeJpeg, result.MimeType);
            Assert.AreEqual(".jpg", result.Extension);
            Assert.AreEqual(640, result.Width);
            Assert.AreEqual(480, result.Height);
            Assert.IsTrue(result.Bytes.Length < source.Length, string.Format("JPEG should be smaller than BMP: {0} vs {1}", result.Bytes.Length, source.Length));
        }

        [TestMethod]
        public void Optimize_PreservesPngTransparency()
        {
            byte[] source = CreatePngWithTransparency(80, 80);
            Assert.IsTrue(ImageUploadOptimizer.HasTransparency(source), "fixture should contain transparency");

            var result = ImageUploadOptimizer.Optimize(source, FullOptions(".png", "image/png"));

            Assert.AreEqual(ImageUploadOptimizer.MimePng, result.MimeType);
            Assert.AreEqual(".png", result.Extension);
            Assert.IsTrue(ImageUploadOptimizer.HasTransparency(result.Bytes), "optimized PNG should keep transparency");

            using (var ms = new MemoryStream(result.Bytes))
            using (var bmp = new Bitmap(ms))
            {
                Color corner = bmp.GetPixel(0, 0);
                Assert.IsTrue(corner.A < 255, "corner pixel should stay transparent, A=" + corner.A);
            }
        }

        [TestMethod]
        public void Optimize_OpaquePngPicksSmallerFormat()
        {
            byte[] source = CreateOpaquePng(400, 300);
            var result = ImageUploadOptimizer.Optimize(source, FullOptions(".png", "image/png"));

            Assert.IsTrue(
                result.MimeType == ImageUploadOptimizer.MimeJpeg || result.MimeType == ImageUploadOptimizer.MimePng,
                "opaque PNG should stay PNG or become JPEG, got " + result.MimeType);
            Assert.IsTrue(result.Bytes.Length > 0);
            Assert.IsTrue(result.Bytes.Length <= source.Length || result.MimeType == ImageUploadOptimizer.MimeJpeg);
        }

        [TestMethod]
        public void Optimize_ThumbnailOptionsStayWithinRequestedBox()
        {
            byte[] source = CreateJpeg(1600, 1200, 90L);
            var options = new ImageUploadOptimizeOptions
            {
                MaxWidth = 400,
                MaxHeight = 400,
                JpegQuality = 75,
                WebPQuality = 75,
                PreferWebP = false,
                KeepOriginalIfSmaller = false,
                SourceExtension = ".jpg",
                SourceMimeType = "image/jpeg",
                ForceExtension = ".jpg"
            };

            var result = ImageUploadOptimizer.Optimize(source, options);
            Assert.IsTrue(result.Width <= 400);
            Assert.IsTrue(result.Height <= 400);
            Assert.AreEqual(400, result.Width);
            Assert.AreEqual(300, result.Height);
        }

        [TestMethod]
        public void Optimize_PreferWebPFallsBackWhenPluginUnavailableOrKeepsWebPWhenSmaller()
        {
            byte[] source = CreateJpeg(1200, 900, 95L);
            var options = FullOptions(".jpg");
            options.PreferWebP = true;

            var result = ImageUploadOptimizer.Optimize(source, options);
            Assert.IsNotNull(result.Bytes);
            Assert.IsTrue(result.Bytes.Length > 0);
            Assert.IsTrue(
                result.MimeType == ImageUploadOptimizer.MimeWebP
                || result.MimeType == ImageUploadOptimizer.MimeJpeg,
                "expected webp or jpeg fallback, got " + result.MimeType);
        }

        [TestMethod]
        public void ForFullImage_UsesInjectedSettingService_NotRootProvider()
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { Constants.ImageUploadMaxWidth, "800" },
                { Constants.ImageUploadMaxHeight, "600" },
                { Constants.ImageUploadJpegQuality, "75" },
                { Constants.ImageUploadWebPQuality, "70" },
                { Constants.ImageUploadPreferWebP, "true" },
                { Constants.ImageUploadKeepOriginalIfSmaller, "false" }
            };
            var settingService = new SettingServiceMockProxy(settings).Service;

            var options = ImageUploadOptimizeOptions.ForFullImage("photo.jpg", "image/jpeg", settingService);

            Assert.AreEqual(800, options.MaxWidth);
            Assert.AreEqual(600, options.MaxHeight);
            Assert.AreEqual(75, options.JpegQuality);
            Assert.AreEqual(70, options.WebPQuality);
            Assert.IsTrue(options.PreferWebP);
            Assert.IsFalse(options.KeepOriginalIfSmaller);
        }

        private class SettingServiceMockProxy : RealProxy
        {
            private readonly Dictionary<string, string> _settings;

            public SettingServiceMockProxy(Dictionary<string, string> settings) : base(typeof(ISettingService))
            {
                _settings = settings;
            }

            public ISettingService Service => (ISettingService)GetTransparentProxy();

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                if (call.MethodName == "GetSettingByKey" && call.InArgs?.Length > 0)
                {
                    var key = call.InArgs[0] as string;
                    string val;
                    _settings.TryGetValue(key ?? string.Empty, out val);
                    return new ReturnMessage(val, null, 0, call.LogicalCallContext, call);
                }

                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }

                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }
        }

        private static ImageUploadOptimizeOptions FullOptions(string extension, string mime = "image/jpeg")
        {
            return new ImageUploadOptimizeOptions
            {
                MaxWidth = 1920,
                MaxHeight = 1920,
                JpegQuality = 82,
                WebPQuality = 82,
                PreferWebP = false,
                KeepOriginalIfSmaller = true,
                SourceExtension = extension,
                SourceMimeType = mime
            };
        }

        private static byte[] CreateJpeg(int width, int height, long quality)
        {
            using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                PaintCatalogLikeContent(g, width, height);
                ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                using (var encoderParams = new EncoderParameters(1))
                {
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    bmp.Save(ms, codec, encoderParams);
                }

                return ms.ToArray();
            }
        }

        private static byte[] CreateBmp(int width, int height)
        {
            using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                PaintCatalogLikeContent(g, width, height);
                bmp.Save(ms, ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        private static byte[] CreateOpaquePng(int width, int height)
        {
            using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                PaintCatalogLikeContent(g, width, height);
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static byte[] CreatePngWithTransparency(int width, int height)
        {
            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(Color.FromArgb(255, 200, 40, 40)))
                {
                    g.FillEllipse(brush, width / 4, height / 4, width / 2, height / 2);
                }

                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static void PaintCatalogLikeContent(Graphics g, int width, int height)
        {
            g.Clear(Color.FromArgb(255, 32, 48, 64));
            using (var brush = new SolidBrush(Color.FromArgb(255, 9, 184, 80)))
            {
                g.FillRectangle(brush, 8, 8, Math.Max(1, width / 2), Math.Max(1, height / 3));
            }

            using (var brush = new SolidBrush(Color.FromArgb(255, 240, 240, 240)))
            {
                g.FillEllipse(brush, width / 3, height / 3, width / 3, height / 3);
            }
        }
    }
}
