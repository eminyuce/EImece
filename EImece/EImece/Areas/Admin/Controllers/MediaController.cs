using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MediaController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string IndexAction = "Index";
        private const string ContentIdKey = "contentId";
        private const string ImageTypeKey = "imageType";
        private FilesHelper filesHelper;

        private Dictionary<string, string> CurrentSelectedModul
        {
            get
            {
                return (Dictionary<string, string>)Session["CurrentSelectedModul"];
            }
            set
            {
                Session["CurrentSelectedModul"] = value;
            }
        }

        public MediaController(FilesHelper filesHelper)
        {
            this.filesHelper = filesHelper;
            this.filesHelper.InitFilesMediaFolder();
        }

        // GET: Admin/Media
        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int? contentId, String mod = null, String imageType = null)
        {
            if (!contentId.HasValue || string.IsNullOrWhiteSpace(mod) || string.IsNullOrWhiteSpace(imageType))
            {
                // Media is opened from a parent entity editor with query params; bare /admin/media/ is not a listing.
                SetErrorMessage("Medya yönetimi bir içerik kaydı üzerinden açılmalıdır.");
                return RedirectToAction(IndexAction, "Dashboard");
            }

            var currentSelectedModul = new Dictionary<string, string>();
            currentSelectedModul.Add(ContentIdKey, contentId + "");
            currentSelectedModul.Add("mod", mod);
            currentSelectedModul.Add(ImageTypeKey, imageType);
            CurrentSelectedModul = currentSelectedModul;

            int id = contentId.Value;
            var returnModel = new MediaAdminIndexModel();
            MediaModType? enumMod = EnumHelper.Parse<MediaModType>(mod);
            EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(imageType);
            if (!enumMod.HasValue || !enumImageType.HasValue)
            {
                SetErrorMessage("Geçersiz medya parametreleri.");
                return RedirectToAction(IndexAction, "Dashboard");
            }

            returnModel.Id = id;
            returnModel.Lang = GetCurrentLanguage;
            returnModel.ImageType = enumImageType.Value;
            returnModel.MediaMod = enumMod.Value;
            returnModel.FileStorages = await FileStorageService.GetUploadImagesAsync(id, enumMod.Value, enumImageType.Value, cancellationToken);
            switch (enumMod.Value)
            {
                case MediaModType.Stories:
                    returnModel.BaseContent = await StoryService.GetSingleAsync(id);
                    break;

                case MediaModType.Products:
                    returnModel.BaseContent = await ProductService.GetSingleAsync(id);
                    break;

                case MediaModType.Menus:
                    returnModel.BaseContent = await MenuService.GetSingleAsync(id);
                    break;

                case MediaModType.MainPageImages:
                    returnModel.BaseContent = await MenuService.GetSingleAsync(id);
                    break;

                default:
                    break;
            }

            // Silent resize defaults for gallery uploads (ImageWidth/Height are NotMapped).
            if (returnModel.BaseContent != null)
            {
                if (returnModel.BaseContent.ImageWidth <= 0)
                {
                    returnModel.BaseContent.ImageWidth = (await SettingService.GetSettingByKeyAsync(Constants.DefaultImageWidth)).ToInt();
                }
                if (returnModel.BaseContent.ImageHeight <= 0)
                {
                    returnModel.BaseContent.ImageHeight = (await SettingService.GetSettingByKeyAsync(Constants.DefaultImageHeight)).ToInt();
                }
            }

            return View(returnModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, int? contentId, String mod = null, String imageType = null)
        {
            if (!contentId.HasValue || string.IsNullOrWhiteSpace(mod) || string.IsNullOrWhiteSpace(imageType))
            {
                return RedirectToAction(IndexAction, "Dashboard");
            }

            if (!Request.IsAjaxRequest() && !ControllerContext.IsChildAction)
            {
                return RedirectToAction("Index", new { contentId, mod, imageType });
            }

            MediaModType? enumMod = EnumHelper.Parse<MediaModType>(mod);
            EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(imageType);
            if (!enumMod.HasValue || !enumImageType.HasValue)
            {
                return RedirectToAction(IndexAction, "Dashboard");
            }

            int id = contentId.Value;
            var fileStorages = await FileStorageService.GetUploadImagesAsync(id, enumMod.Value, enumImageType.Value, cancellationToken);
            ViewBag.ContentId = id;
            ViewBag.MediaMod = enumMod.Value;
            ViewBag.ImageType = enumImageType.Value;
            return new QueryableResult<FileStorage>((fileStorages ?? new List<FileStorage>()).AsQueryable());
        }

        public ActionResult Show(int id, String mod, String imageType)
        {
            var CurrentContext = HttpContext;
            JsonFiles ListOfFiles = filesHelper.GetFileList(CurrentContext);
            var model = new FilesViewModel()
            {
                Files = ListOfFiles.files
            };

            return View(model);
        }

        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public async Task<JsonResult> Upload()
        {
            int Id = Request.Form[ContentIdKey].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(Request.Form[ImageTypeKey].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(Request.Form["mod"].ToStr());
            int imageHeight = Request.Form["imageHeight"].ToInt();
            int imageWidth = Request.Form["imageWidth"].ToInt();

            string selectedTags = Request.Form["selectedTags"];

            var resultList = new List<ViewDataUploadFilesResult>();

            var CurrentContext = HttpContext;

            filesHelper.UploadAndShowResults(CurrentContext, resultList);
            JsonFiles files = new JsonFiles(resultList);

            bool isEmpty = !resultList.Any();
            if (isEmpty)
            {
                return Json(AdminResource.UploadError);
            }
            else
            {
                await FileStorageService.SaveUploadImagesAsync(Id, imageType, mod, resultList, CurrentLanguage, selectedTags);
                return Json(files);
            }
        }

        public JsonResult GetFileList()
        {
            var CurrentContext = HttpContext;
            var list = filesHelper.GetFileList(CurrentContext);

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [DeleteAuthorize()]
        public JsonResult DeleteFile(string file, int contentId, String mod, String imageType)
        {
            var CurrentContext = HttpContext;
            filesHelper.DeleteFile(file, CurrentContext);

            return Json("OK");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fileStorage = await FileStorageService.GetSingleAsync(id);
            if (fileStorage == null)
            {
                return HttpNotFound();
            }
            try
            {
                int contentId = CurrentSelectedModul[ContentIdKey].ToInt();
                var returnModel = new MediaAdminIndexModel();
                MediaModType? enumMod = EnumHelper.Parse<MediaModType>(CurrentSelectedModul["mod"]);
                EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(CurrentSelectedModul[ImageTypeKey]);

                await FileStorageService.DeleteUploadImageAsync(id, contentId, enumImageType, enumMod);
                SetSuccessMessage();
                return RedirectToAction(IndexAction,
                    new
                    {
                        contentId = CurrentSelectedModul[ContentIdKey],
                        mod = CurrentSelectedModul["mod"],
                        imageType = CurrentSelectedModul[ImageTypeKey]
                    }
                );
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete fileStorage:" + ex.StackTrace, fileStorage);
                SetErrorMessage();
                return RedirectToAction(IndexAction,
                    new
                    {
                        contentId = CurrentSelectedModul[ContentIdKey],
                        mod = CurrentSelectedModul["mod"],
                        imageType = CurrentSelectedModul[ImageTypeKey]
                    }
                );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize]
        public async Task<ActionResult> DeleteMissingFiles(CancellationToken cancellationToken, int? contentId, string mod = null, string imageType = null)
        {
            int id = contentId.HasValue && contentId.Value > 0
                ? contentId.Value
                : (CurrentSelectedModul.ContainsKey(ContentIdKey) ? CurrentSelectedModul[ContentIdKey].ToInt() : 0);

            string modStr = !string.IsNullOrWhiteSpace(mod)
                ? mod
                : (CurrentSelectedModul.ContainsKey("mod") ? CurrentSelectedModul["mod"] : null);

            string imageTypeStr = !string.IsNullOrWhiteSpace(imageType)
                ? imageType
                : (CurrentSelectedModul.ContainsKey(ImageTypeKey) ? CurrentSelectedModul[ImageTypeKey] : null);

            MediaModType? enumMod = EnumHelper.Parse<MediaModType>(modStr);
            EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(imageTypeStr);

            if (id <= 0 || !enumMod.HasValue || !enumImageType.HasValue)
            {
                return RedirectToAction(IndexAction, new { contentId = id, mod = modStr, imageType = imageTypeStr });
            }

            try
            {
                int deletedCount = await FileStorageService.DeleteMissingFilesAsync(id, enumMod.Value, enumImageType.Value, cancellationToken).ConfigureAwait(false);
                if (deletedCount > 0)
                {
                    SetSuccessMessage(string.Format(AdminResource.DeleteMissingFilesSuccessFormat, deletedCount));
                }
                else
                {
                    SetStatusMessage(AdminResource.DeleteMissingFilesNoneFound, "info");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error deleting missing files for contentId={0}, mod={1}, imageType={2}", id, modStr, imageTypeStr);
                SetErrorMessage();
            }

            return RedirectToAction(IndexAction, new { contentId = id, mod = modStr, imageType = imageTypeStr });
        }
    }
}
