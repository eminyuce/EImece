using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using EImece.Web.Controllers;
using EImece.Web.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ImagesController : BaseController
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IEimeceCacheProvider _memoryCacheProvider;
        private readonly FilesHelper _filesHelper;
        public IFileStorageService FileStorageService => _fileStorageService;
        public IEimeceCacheProvider MemoryCacheProvider => _memoryCacheProvider;
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.InitFilesMediaFolder();
                return _filesHelper;
            }
        }

        public ImagesController(ISettingService settingService,
            AutoMapper.IMapper mapper,
            IFileStorageService fileStorageService,
            IEimeceCacheProvider memoryCacheProvider,
            FilesHelper filesHelper, ILogger<ImagesController> logger)
            : base(settingService, mapper, logger)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _memoryCacheProvider = memoryCacheProvider ?? throw new ArgumentNullException(nameof(memoryCacheProvider));
            _filesHelper = filesHelper ?? throw new ArgumentNullException(nameof(filesHelper));
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

            id = id.Trim().TrimEnd('/');
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

                try
                {
                    bool wantsWebP = Request.AcceptTypes != null
                        && Request.AcceptTypes.Any(t => t != null && t.IndexOf("image/webp", StringComparison.OrdinalIgnoreCase) >= 0);

                    // Fetch metadata to check conditional request headers (If-None-Match / If-Modified-Since) before processing
                    var fileStorage = await _fileStorageService.GetFileStorageAsync(fileStorageId).ConfigureAwait(false);
                    if (fileStorage != null)
                    {
                        var updatedDate = fileStorage.UpdatedDate > DateTime.MinValue ? fileStorage.UpdatedDate : fileStorage.CreatedDate;
                        var formatTag = wantsWebP ? "webp" : "orig";
                        var etag = string.Format("\"{0}-{1}-{2}-{3}\"", fileStorageId, imageSize, formatTag, updatedDate.Ticks);

                        // Check conditional If-None-Match header
                        var incomingEtag = Request.Headers["If-None-Match"];
                        if (!string.IsNullOrEmpty(incomingEtag) && string.Equals(incomingEtag.Trim('\"'), etag.Trim('\"'), StringComparison.OrdinalIgnoreCase))
                        {
                            ApplyLongLivedImageCache(updatedDate, etag);
                            return new HttpStatusCodeResult(HttpStatusCode.NotModified);
                        }

                        // Check conditional If-Modified-Since header
                        var incomingIfMod = Request.Headers["If-Modified-Since"];
                        if (!string.IsNullOrEmpty(incomingIfMod) && DateTime.TryParse(incomingIfMod, out var ifModDate) && updatedDate.ToUniversalTime() <= ifModDate.ToUniversalTime().AddSeconds(1))
                        {
                            ApplyLongLivedImageCache(updatedDate, etag);
                            return new HttpStatusCodeResult(HttpStatusCode.NotModified);
                        }

                        var imageByte = wantsWebP
                            ? await FilesHelper.GetResizedImageAsWebPAsync(fileStorageId, width, height)
                            : await FilesHelper.GetResizedImageAsync(fileStorageId, width, height);
                        if (imageByte != null && imageByte.ImageBytes != null)
                        {
                            Response.StatusCode = 200;
                            ApplyLongLivedImageCache(updatedDate, etag);
                            return File(imageByte.ImageBytes, imageByte.ContentType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to generate image id={0} size={1}", id, imageSize);
                }

                return this.GetDefaultFileContentResult((string)imageSize);
            }
            else
            {
                return new EmptyResult();
            }
        }

        private void ApplyLongLivedImageCache(DateTime updatedDated, string etag = null)
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(365));
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(TimeSpan.FromDays(365));
            Response.Cache.SetSlidingExpiration(true);
            Response.Cache.SetOmitVaryStar(true);
            Response.Cache.SetValidUntilExpires(true);
            Response.Headers.Set("Vary", "Accept, Accept-Encoding");
            Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            if (updatedDated > DateTime.MinValue)
            {
                Response.Cache.SetLastModified(updatedDated.ToUniversalTime());
            }
            if (!string.IsNullOrEmpty(etag))
            {
                Response.Cache.SetETag(etag);
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

            return this.File(fileContents, MediaTypeNames.Image.Jpeg);
        }

        // Legacy arithmetic CAPTCHA image (used when CaptchaProvider=Legacy)
        public ActionResult GetCaptcha(string prefix, bool noisy = true)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            var rand = new Random((int)DateTime.Now.Ticks);
            int a = rand.Next(1, 5);
            int b = rand.Next(1, 5);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            Session["Captcha" + prefix] = a + b;

            FileContentResult img = null;
            try
            {
                img = this.File(FilesHelper.GenerateCaptchaImg(captcha, true), MediaTypeNames.Image.Jpeg);
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
            var cacheKey = CacheKeys.WebSiteLogoImage;
            FileContentResult result = null;
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                var webSiteLogo = await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteLogo);
                if (webSiteLogo == null || string.IsNullOrWhiteSpace(webSiteLogo.SettingValue))
                {
                    Logger.LogWarning("WebSiteLogo setting is empty; serving default placeholder logo.");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    return GetDefaultImage("w200h60");
                }

                var p = FilesHelper.GetFileNames2(webSiteLogo.SettingValue);
                var isFullFileExits = System.IO.File.Exists(p.Item1);
                if (isFullFileExits)
                {
                    var fileBytes = System.IO.File.ReadAllBytes(p.Item1);
                    result = File(fileBytes, MediaTypeNames.Image.Jpeg);
                    MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheVeryLongSeconds);
                }
            }

            if (result == null)
            {
                Logger.LogWarning("WebSiteLogo setting or file is missing; serving default placeholder logo.");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                return GetDefaultImage("w200h60");
            }

            ApplyLongLivedImageCache(DateTime.UtcNow.Date, "\"logo-v1\"");
            return result;
        }
    }
}