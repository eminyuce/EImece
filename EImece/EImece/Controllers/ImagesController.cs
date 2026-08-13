using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ImagesController : BaseController
    {
        private const string ContentType = "image/Jpeg";
        private IEimeceCacheProvider _memoryCacheProvider { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        [Inject]
        public IEimeceCacheProvider MemoryCacheProvider
        {
            get
            {
                return _memoryCacheProvider;
            }
            set
            {
                _memoryCacheProvider = value;
            }
        }

        private FilesHelper _filesHelper { get; set; }

        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.InitFilesMediaFolder();
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }

        // GET: Images
        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = Constants.ImageProxyCaching)]
        public async Task<ActionResult> Index(String id, String imageSize)
        {
            return await GenerateImageAsync(id, imageSize);
        }

        private async Task<ActionResult> GenerateImageAsync(string id, string imageSize)
        {
            if (String.IsNullOrEmpty(id))
            {
                return Content("Id cannot be null");
            }

            var fileStorageId = id.Replace(".jpg", "").GetId();

            if (fileStorageId > 0)
            {
                int height = 0;
                int width = 0;
                if (String.IsNullOrEmpty(imageSize))
                {
                    imageSize = "w150h150";
                }

                width = Regex.Match(imageSize, @"w(\d*)").Value.Replace("w", "").ToInt();
                height = Regex.Match(imageSize, @"h(\d*)").Value.Replace("h", "").ToInt();

                bool wantsWebP = Request.AcceptTypes != null
                    && Request.AcceptTypes.Any(t => t != null && t.IndexOf("image/webp", StringComparison.OrdinalIgnoreCase) >= 0);
                var imageByte = wantsWebP
                    ? await FilesHelper.GetResizedImageAsWebPAsync(fileStorageId, width, height)
                    : await FilesHelper.GetResizedImageAsync(fileStorageId, width, height);
                if (imageByte != null && imageByte.ImageBytes != null)
                {
                    Response.StatusCode = 200;
                    ApplyLongLivedImageCache(imageByte.UpdatedDated);
                    return File(imageByte.ImageBytes, imageByte.ContentType);
                }
                else
                {
                    return this.GetDefaultFileContentResult((string)imageSize);
                }
            }
            else
            {
                return new EmptyResult();
            }
        }

        private void ApplyLongLivedImageCache(DateTime updatedDated)
        {
            Response.Cache.SetExpires(DateTime.Now.AddDays(365));
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(TimeSpan.FromDays(365));
            Response.Cache.SetSlidingExpiration(true);
            Response.Cache.SetOmitVaryStar(true);
            Response.Cache.SetValidUntilExpires(true);
            Response.Headers.Set("Vary",
                string.Join(",", new string[] { "Accept", "Accept-Encoding" }));
            Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            if (updatedDated != null && updatedDated > DateTime.MinValue)
            {
                Response.Cache.SetLastModified(updatedDated.ToLocalTime());
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = Constants.ImageProxyCaching)]
        public FileContentResult DefaultImage(String imageSize)
        {
            return this.GetDefaultFileContentResult((string)imageSize);
        }

        private FileContentResult GetDefaultFileContentResult(string imageSize)
        {
            return GetDefaultImage(imageSize);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = Constants.ImageProxyCaching)]
        public FileContentResult GetDefaultImage(String imageSize)
        {
            int height = 0;
            int width = 0;
            if (String.IsNullOrEmpty(imageSize))
            {
                imageSize = "w150h150";
            }

            width = Regex.Match(imageSize, @"w(\d*)").Value.Replace("w", "").ToInt();
            height = Regex.Match(imageSize, @"h(\d*)").Value.Replace("h", "").ToInt();

            if (width == 0 && height > 0)
            {
                width = height;
            }

            if (height == 0)
            {
                height = width;
            }

            if (width == 0 && height == 0)
            {
                width = 300;
                height = 400;
            }
            var timer = new Stopwatch();
            timer.Start();
            byte[] fileContents = FilesHelper.GenerateDefaultImg(Constants.DefaultImageText, width, height);
            timer.Stop();
            //Logger.Info("FilesHelper.GenerateDefaultImg width:" + width + " height:" + height + " timer:" + timer.ElapsedMilliseconds);

            return this.File(fileContents, ContentType);
        }

        // Legacy arithmetic CAPTCHA image (used when CaptchaProvider=Legacy)
        public ActionResult GetCaptcha(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            int a = rand.Next(1, 5);
            int b = rand.Next(1, 5);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            Session["Captcha" + prefix] = a + b;

            FileContentResult img = null;
            try
            {
                img = this.File(FilesHelper.GenerateCaptchaImg(captcha, true), ContentType);
            }
            catch
            {
            }

            return img;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = Constants.ImageProxyCaching)]
        public async Task<ActionResult> Logo()
        {
            var cacheKey = String.Format("WebSiteLogo");
            FileContentResult result = null;
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                var webSiteLogo = await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteLogo);
                if (webSiteLogo == null || string.IsNullOrWhiteSpace(webSiteLogo.SettingValue))
                {
                    // Avoid OutputCache storing a miss while the logo file/setting is temporarily absent.
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.TrySkipIisCustomErrors = true;
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                var p = FilesHelper.GetFileNames2(webSiteLogo.SettingValue);
                var isFullFileExits = System.IO.File.Exists(p.Item1);
                if (isFullFileExits)
                {
                    var ms = new MemoryStream(System.IO.File.ReadAllBytes(p.Item1));
                    result = File(ms.ToArray(), ContentType);
                    ms.Dispose();
                    MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheVeryLongSeconds);
                }
                else
                {
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.TrySkipIisCustomErrors = true;
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }

            if (result == null)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.TrySkipIisCustomErrors = true;
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return result;
        }
    }
}