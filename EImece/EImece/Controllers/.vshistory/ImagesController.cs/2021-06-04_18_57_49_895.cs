using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        public async Task<ActionResult> Index333333(String id, String imageSize)
        {
            return await Task.Run(() =>
            {
                return GenerateImage(id, imageSize);
            });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = Constants.ImageProxyCaching)]
        public ActionResult Index(String id, String imageSize)
        {
            return GenerateImage(id, imageSize);
        }

        private ActionResult GenerateImage(string id, string imageSize)
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

                var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
                if (imageByte != null && imageByte.ImageBytes != null)
                {
                    Response.StatusCode = 200;
                    Response.Cache.SetExpires(DateTime.Now.AddDays(365));
                    Response.Cache.SetCacheability(HttpCacheability.Public);
                    Response.Cache.SetMaxAge(TimeSpan.FromDays(365));
                    Response.Cache.SetSlidingExpiration(true);
                    Response.Cache.SetOmitVaryStar(true);
                    Response.Headers.Set("Vary",
                        string.Join(",", new string[] { "Accept", "Accept-Encoding" }));
                    if (imageByte.UpdatedDated != null)
                    {
                        Response.Cache.SetLastModified(imageByte.UpdatedDated.ToLocalTime());
                    }
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

        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = Constants.ImageProxyCaching)]
        public async Task<FileContentResult> DefaultImage(String imageSize)
        {
            return await Task.Run(() =>
            {
                return this.GetDefaultFileContentResult((string)imageSize);
            }).ConfigureAwait(true);
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
            Logger.Info("FilesHelper.GenerateDefaultImg width:" + width + " height:" + height + " timer:" + timer.ElapsedMilliseconds);

            return this.File(fileContents, ContentType);
        }

        public ActionResult GetModifiedImage(String id, String imageSize)
        {
            int height = 0;
            int width = 0;
            if (String.IsNullOrEmpty(imageSize))
            {
                imageSize = "w150h150";
            }

            width = Regex.Match(imageSize, @"w(\d*)").Value.Replace("w", "").ToInt();
            height = Regex.Match(imageSize, @"h(\d*)").Value.Replace("h", "").ToInt();

            var fileStorageId = id.Replace(".jpg", "").ToInt();
            var savedImage = FilesHelper.GetResizedImage(fileStorageId, width, height);
            Image image = Image.FromStream(new MemoryStream(savedImage.ImageBytes));

            using (Graphics g = Graphics.FromImage(image))
            {
                // do something with the Graphics (eg. write "Hello World!")
                string text = "Hello World!";

                // Create font and brush.
                Font drawFont = new Font("Arial", 10);
                SolidBrush drawBrush = new SolidBrush(Color.Black);

                // Create point for upper-left corner of drawing.
                PointF stringPoint = new PointF(0, 0);

                g.DrawString(text, drawFont, drawBrush, stringPoint);
            }

            MemoryStream ms = new MemoryStream();

            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return File(ms.ToArray(), savedImage.ContentType);
        }

        //controller function to generate image
        //returns image file (through url.action in img src attribute)
        public ActionResult GetCaptcha(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question
            int a = rand.Next(1, 5);
            int b = rand.Next(1, 5);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            //store answer
            Session["Captcha" + prefix] = a + b;

            //image stream
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
        public async Task<FileContentResult> Logo()
        {
            return await Task.Run(() =>
            {
                var cacheKey = String.Format("WebSiteLogo");
                FileContentResult result = null;
                if (!MemoryCacheProvider.Get(cacheKey, out result))
                {
                    var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
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
                        var response = HttpContext.Response;
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        response.TrySkipIisCustomErrors = true;
                    }
                }

                return result;
            }).ConfigureAwait(true);
        }
    }
}