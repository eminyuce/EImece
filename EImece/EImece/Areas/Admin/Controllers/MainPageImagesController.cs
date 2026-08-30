using EImece.Web.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Web.Helpers;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
using EImece.Domain.Models.Enums;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class MainPageImagesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IMainPageImageService MainPageImageService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected FilesHelper FilesHelper { get; }

        public MainPageImagesController(
            ISettingService settingService,
            IMainPageImageService mainPageImageService,
            IEntityFactory entityFactory,
            FilesHelper filesHelper)
            : base(settingService)
        {
            MainPageImageService = mainPageImageService ?? throw new ArgumentNullException(nameof(mainPageImageService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            FilesHelper = filesHelper ?? throw new ArgumentNullException(nameof(filesHelper));
        }

        // GET: Admin/MainPageImages
        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<MainPageImage, bool>> whereLambda = r => r.Name.Contains(search);
            var mainPageImages = await MainPageImageService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(mainPageImages);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<MainPageImage, bool>> whereLambda = r => r.Name.Contains(search);
            var mainPageImages = await MainPageImageService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<MainPageImage>(mainPageImages.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<MainPageImage>();

            if (id == 0)
            {
            }
            else
            {
                content = await MainPageImageService.GetBaseContentAsync(id, cancellationToken);
            }

            return View(content);
        }

        //
        // POST: /StoryCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(MainPageImage mainpageimage, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (mainpageimage == null)
                {
                    return HttpNotFound();
                }
                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(
                      postedImage,
                      mainpageimage.ImageHeight,
                      mainpageimage.ImageWidth,
                      EImeceImageType.MainPageImages,
                      mainpageimage);

                    mainpageimage.ImageState = true;
                    await MainPageImageService.SaveOrEditEntityAsync(mainpageimage);
                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, mainpageimage);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            RemoveModelState();
            return View(mainpageimage);
        }

        // POST: Admin/MainPageImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                await MainPageImageService.DeleteMainPageImageAsync(id);
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete MainPageImages:" + ex.StackTrace, id);
                SetErrorMessage();
                return RedirectToAction("Index");
            }
        }
    }
}
